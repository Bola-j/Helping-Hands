import json
import os
import unittest
from urllib.parse import urlsplit

from playwright.sync_api import expect, sync_playwright


class NavigationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch(headless=True)
        cls.base = os.environ.get('WEB_TEST_URL', 'http://localhost:8081')

    @classmethod
    def tearDownClass(cls):
        cls.browser.close()
        cls.playwright.stop()

    def test_navigation_and_browser_history(self):
        page = self.browser.new_page()
        page.goto(self.base, wait_until='networkidle')
        page.get_by_role('link', name='See how it works').click()
        expect(page.get_by_role('heading', name='How it works')).to_be_visible()
        page.reload(wait_until='networkidle')
        expect(page.get_by_text('A person makes a request')).to_be_visible()
        page.go_back(wait_until='networkidle')
        expect(page.get_by_role('heading', name='Coordinate requests for assistance.')).to_be_visible()
        page.close()

    def test_mobile_layout_and_keyboard_navigation(self):
        page = self.browser.new_page(viewport={'width': 375, 'height': 812})
        page.goto(self.base, wait_until='networkidle')
        self.assertLessEqual(page.evaluate('document.documentElement.scrollWidth'), 375)
        page.keyboard.press('Tab')
        self.assertEqual(page.evaluate('document.activeElement.getAttribute("aria-label")'), 'Helping Hands home')
        page.keyboard.press('Tab')
        page.keyboard.press('Enter')
        expect(page.get_by_role('heading', name='How it works')).to_be_visible()
        page.close()

    def test_demo_page_renders_on_mobile(self):
        page = self.browser.new_page(viewport={'width': 375, 'height': 812})
        page.goto(self.base + '/demo', wait_until='networkidle')
        expect(page.get_by_role('heading', name='Beneficiary request demo')).to_be_visible()
        self.assertLessEqual(page.evaluate('document.documentElement.scrollWidth'), 375)
        page.close()


class EmailValidationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch(headless=True)
        cls.base = os.environ.get('WEB_TEST_URL', 'http://localhost:8081')

    @classmethod
    def tearDownClass(cls):
        cls.browser.close()
        cls.playwright.stop()

    def _mock_auth_api(self, page):
        requests = []
        signed_in = {'value': False}

        def handle(route):
            request = route.request
            path = urlsplit(request.url).path
            if path == '/api/v1/auth/antiforgery':
                route.fulfill(status=200, content_type='application/json', body=json.dumps({
                    'requestToken': 'fictional-csrf-token', 'headerName': 'X-CSRF-TOKEN',
                }))
            elif path == '/api/v1/auth/me':
                if signed_in['value']:
                    route.fulfill(status=200, content_type='application/json', body=json.dumps({
                        'id': 'fictional-account-id', 'displayName': 'Fictional User',
                        'email': 'person@example.test', 'roles': [],
                    }))
                else:
                    route.fulfill(status=401, content_type='application/problem+json', body=json.dumps({
                        'title': 'Unauthorized', 'status': 401,
                    }))
            elif path in (
                '/api/v1/auth/register/beneficiary',
                '/api/v1/auth/register/charity',
                '/api/v1/auth/login',
            ):
                requests.append({'path': path, 'payload': request.post_data_json})
                if path == '/api/v1/auth/login':
                    signed_in['value'] = True
                    route.fulfill(status=204, body='')
                elif path.endswith('/beneficiary'):
                    route.fulfill(status=201, content_type='application/json', body=json.dumps({
                        'accountId': 'fictional-account-id', 'beneficiaryId': 'fictional-beneficiary-id',
                        'reference': 'BEN-EMAIL-TEST',
                    }))
                else:
                    route.fulfill(status=201, content_type='application/json', body=json.dumps({
                        'accountId': 'fictional-account-id', 'recipientId': 'fictional-recipient-id',
                        'reference': 'REC-EMAIL-TEST',
                    }))
            else:
                route.continue_()

        page.route('**/api/v1/**', handle)
        return requests

    def _check_email_field(self, path, email_label, setup_values, submit_label, endpoint, payload_key, success_heading):
        page = self.browser.new_page()
        requests = self._mock_auth_api(page)
        try:
            page.goto(self.base + path, wait_until='networkidle')
            for label, value in setup_values:
                locator = page.get_by_role('textbox', name=label) if label == 'Password' else page.get_by_label(label, exact=True)
                locator.fill(value)

            if path == '/register/beneficiary':
                contact_details = page.get_by_label('Contact details')
                self.assertEqual(contact_details.evaluate('(element) => element.tagName'), 'TEXTAREA')
            elif path == '/register/charity':
                registration_information = page.get_by_label('Registration information')
                self.assertEqual(registration_information.evaluate('(element) => element.tagName'), 'TEXTAREA')

            field = page.get_by_label(email_label, exact=True)
            expect(field).to_be_visible()
            self.assertEqual(field.get_attribute('type'), 'email')
            self.assertIsNotNone(field.get_attribute('required'))

            field.fill('')
            page.get_by_role('button', name=submit_label).click()
            expect(field).to_have_attribute('aria-invalid', 'true')
            error_id = field.get_attribute('id') + '-error'
            self.assertIn(error_id, (field.get_attribute('aria-describedby') or '').split())
            expect(page.locator(f'#{error_id}')).to_have_text('Enter your email address.')
            self.assertEqual(requests, [])

            field.fill('not-an-email')
            page.get_by_role('button', name=submit_label).click()
            expect(field).to_have_attribute('aria-invalid', 'true')
            expect(page.locator(f'#{error_id}')).to_have_text('Enter an email address like name@example.org.')
            self.assertEqual(requests, [])

            valid_email = 'fictional@example.test'
            field.fill(valid_email)
            page.get_by_role('button', name=submit_label).click()
            expect(page.get_by_role('heading', name=success_heading)).to_be_visible()
            self.assertEqual(len(requests), 1)
            self.assertEqual(requests[0]['path'], endpoint)
            self.assertEqual(requests[0]['payload'][payload_key], valid_email)
        finally:
            page.close()

    def test_beneficiary_and_charity_signup_email_fields(self):
        cases = (
            (
                '/register/beneficiary', 'Email',
                [('Your name', 'Fictional Beneficiary'), ('Password', 'FictionalPassword123!')],
                'Create account', '/api/v1/auth/register/beneficiary', 'email', 'Your account is created',
            ),
            (
                '/register/charity', 'Email',
                [('Your name', 'Fictional Representative'), ('Password', 'FictionalPassword123!'),
                 ('Organization name', 'Fictional Food Bank'), ('Organization contact email', 'contact@example.test')],
                'Submit application', '/api/v1/auth/register/charity', 'email', 'Application received',
            ),
            (
                '/register/charity', 'Organization contact email',
                [('Your name', 'Fictional Representative'), ('Password', 'FictionalPassword123!'),
                 ('Organization name', 'Fictional Food Bank'), ('Email', 'representative@example.test')],
                'Submit application', '/api/v1/auth/register/charity', 'contactDetails', 'Application received',
            ),
        )
        for case in cases:
            with self.subTest(field=case[1]):
                self._check_email_field(*case)

    def test_login_email_field(self):
        self._check_email_field(
            '/login', 'Email', [('Password', 'FictionalPassword123!')],
            'Sign in', '/api/v1/auth/login', 'email', 'You are signed in',
        )


if __name__ == '__main__':
    unittest.main()
