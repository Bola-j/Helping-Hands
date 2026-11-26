"""Exercise the /demo beneficiary flow in a browser against an explicitly isolated local HTTPS stack.

Set SESSION_TEST_URL to the isolated proxy origin and SESSION_TEST_ALLOW_WRITE to
"isolated". This creates one fictional beneficiary and one request in that stack's database.
"""
import os
import secrets
import unittest
import uuid
from urllib.parse import urlsplit

from playwright.sync_api import expect, sync_playwright


class LiveDemoFlowTests(unittest.TestCase):
    def test_register_sign_in_submit_and_sign_out(self):
        origin = os.environ.get('SESSION_TEST_URL', '')
        url = urlsplit(origin)
        if (os.environ.get('SESSION_TEST_ALLOW_WRITE') != 'isolated' or
                url.scheme != 'https' or url.hostname not in ('localhost', '127.0.0.1') or
                url.path not in ('', '/') or url.query or url.fragment):
            self.fail('Set SESSION_TEST_URL to the isolated local HTTPS proxy and SESSION_TEST_ALLOW_WRITE=isolated')

        email = f"fictional-{uuid.uuid4().hex}@example.invalid"
        password = secrets.token_urlsafe(20) + 'Aa1!'
        description = 'Fictional need: winter blankets for a household of four.'
        with sync_playwright() as playwright:
            browser = playwright.chromium.launch(headless=True)
            context = browser.new_context(ignore_https_errors=True)
            page = context.new_page()
            page.goto(origin.rstrip('/') + '/demo', wait_until='networkidle')
            auth_submit = page.locator('form.demo-card button[type="submit"]')

            # Register
            page.get_by_label('Display name').fill('Fictional Beneficiary')
            page.get_by_label('Email').fill(email)
            page.get_by_label('Password').fill(password)
            auth_submit.click()
            expect(page.get_by_role('status')).to_contain_text('Registration complete')

            # Wrong password is rejected
            page.get_by_label('Password').fill('IncorrectPassword1!')
            auth_submit.click()
            expect(page.get_by_role('alert')).to_be_visible()

            # Correct sign-in
            page.get_by_label('Password').fill(password)
            auth_submit.click()
            expect(page.get_by_role('heading', name='Welcome, Fictional Beneficiary')).to_be_visible()

            # Submit a single-need request and see it listed
            page.get_by_label('Describe the one need').fill(description)
            page.get_by_label('I am authorized').check()
            page.get_by_label('I agree that these details').check()
            page.get_by_role('button', name='Submit request').click()
            expect(page.locator('.demo-list li')).to_contain_text(description)

            # Sign out returns to the form
            page.get_by_role('button', name='Sign out').click()
            expect(page.get_by_role('button', name='Create account')).to_be_visible()
            browser.close()


if __name__ == '__main__':
    unittest.main()