"""Exercise the real session client against an explicitly isolated local HTTPS stack.

Set SESSION_TEST_URL to the isolated proxy origin and SESSION_TEST_ALLOW_WRITE to
"isolated". This creates one fictional beneficiary in that stack's database.
"""
import os
import secrets
import subprocess
import unittest
import uuid
from pathlib import Path
from urllib.parse import urlsplit

from playwright.sync_api import sync_playwright


HERE = Path(__file__).resolve().parent


class LiveSessionTests(unittest.TestCase):
    def test_fictional_account_session_through_https_proxy(self):
        origin = os.environ.get('SESSION_TEST_URL', '')
        url = urlsplit(origin)
        if (os.environ.get('SESSION_TEST_ALLOW_WRITE') != 'isolated' or
                url.scheme != 'https' or url.hostname not in ('localhost', '127.0.0.1') or
                url.path not in ('', '/') or url.query or url.fragment):
            self.fail('Set SESSION_TEST_URL to the isolated local HTTPS proxy and SESSION_TEST_ALLOW_WRITE=isolated')

        # Only compiled test code is injected; all requests use the real client module.
        bundle = subprocess.run(['node', str(HERE / 'bundle_session.cjs')], check=True,
                                capture_output=True, text=True).stdout
        email = f"fictional-{uuid.uuid4().hex}@example.invalid"
        password = secrets.token_urlsafe(20) + 'Aa1!'
        with sync_playwright() as playwright:
            browser = playwright.chromium.launch(headless=True)
            context = browser.new_context(ignore_https_errors=True)
            page = context.new_page()
            page.goto(origin, wait_until='networkidle')
            page.add_script_tag(content=bundle)
            result = page.evaluate('''async ({email, password}) => {
                const { apiFetch } = window.sessionTestClient;
                const capture = async (action) => {
                  try { return { status: (await action()).status }; }
                  catch (error) { return { status: error.status, kind: error.kind }; }
                };
                const anonymous = await capture(() => apiFetch('/api/v1/auth/me'));
                const signup = await capture(() => apiFetch('/api/v1/auth/register/beneficiary', {
                  method: 'POST', headers: {'Content-Type': 'application/json'},
                  body: JSON.stringify({ displayName: 'Fictional Beneficiary', email, password, isFamily: false })
                }));
                const afterSignup = await capture(() => apiFetch('/api/v1/auth/me'));
                const badLogin = await capture(() => apiFetch('/api/v1/auth/login', {
                  method: 'POST', headers: {'Content-Type': 'application/json'},
                  body: JSON.stringify({ email, password: 'IncorrectPassword1!' })
                }));
                return {anonymous, signup, afterSignup, badLogin};
            }''', {'email': email, 'password': password})
            before_names = {cookie['name'] for cookie in context.cookies()}
            login = page.evaluate('''async ({email, password}) => {
                const response = await window.sessionTestClient.apiFetch('/api/v1/auth/login', {
                  method: 'POST', headers: {'Content-Type': 'application/json'},
                  body: JSON.stringify({ email, password })
                });
                return response.status;
            }''', {'email': email, 'password': password})
            auth_cookie_secure = any(
                cookie['name'] not in before_names and cookie['secure'] and cookie['httpOnly'] and
                cookie['sameSite'] == 'Strict' for cookie in context.cookies()
            )
            after = page.evaluate('''async () => {
                const { apiFetch } = window.sessionTestClient;
                const capture = async (action) => {
                  try { return { status: (await action()).status }; }
                  catch (error) { return { status: error.status, kind: error.kind }; }
                };
                const me = await apiFetch('/api/v1/auth/me');
                const user = await me.json();
                const forbidden = await capture(() => apiFetch('/api/v1/catalog/categories'));
                const afterForbidden = await capture(() => apiFetch('/api/v1/auth/me'));
                const logout = await capture(() => apiFetch('/api/v1/auth/logout', { method: 'POST' }));
                const afterLogout = await capture(() => apiFetch('/api/v1/auth/me'));
                return { role: user.roles.includes('Beneficiary'), forbidden, afterForbidden, logout, afterLogout };
            }''')
            self.assertEqual(result['anonymous'], {'status': 401, 'kind': 'unauthorized'})
            self.assertIn(result['signup']['status'], (200, 201))
            self.assertEqual(result['afterSignup']['status'], 401)
            self.assertEqual(result['badLogin'], {'status': 401, 'kind': 'unauthorized'})
            self.assertEqual(login, 204)
            self.assertTrue(auth_cookie_secure)
            self.assertTrue(after['role'])
            self.assertEqual(after['forbidden'], {'status': 403, 'kind': 'forbidden'})
            self.assertEqual(after['afterForbidden']['status'], 200)
            self.assertEqual(after['logout']['status'], 204)
            self.assertEqual(after['afterLogout']['status'], 401)
            cookies = context.cookies()
            self.assertTrue(any(cookie['secure'] and cookie['httpOnly'] and cookie['sameSite'] == 'Strict'
                                for cookie in cookies))
            context.close()
            browser.close()


if __name__ == '__main__':
    unittest.main()
