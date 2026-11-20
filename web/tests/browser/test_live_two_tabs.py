"""Check stale-tab writes on an explicitly isolated local HTTPS stack.

Requires SESSION_TEST_URL and SESSION_TEST_ALLOW_WRITE=isolated. The test
creates two fictional beneficiaries in the target database.
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


class LiveTwoTabTests(unittest.TestCase):
    def test_stale_tab_cannot_log_out_the_new_account(self):
        origin = os.environ.get('SESSION_TEST_URL', '')
        url = urlsplit(origin)
        if (os.environ.get('SESSION_TEST_ALLOW_WRITE') != 'isolated' or
                url.scheme != 'https' or url.hostname not in ('localhost', '127.0.0.1') or
                url.path not in ('', '/') or url.query or url.fragment):
            self.fail('Set SESSION_TEST_URL to the isolated local HTTPS proxy and SESSION_TEST_ALLOW_WRITE=isolated')
        bundle = subprocess.run(['node', str(HERE / 'bundle_session.cjs')], check=True,
                                capture_output=True, text=True).stdout
        accounts = [{'email': f'fictional-{uuid.uuid4().hex}@example.invalid',
                     'password': secrets.token_urlsafe(20) + 'Aa1!'} for _ in range(2)]
        with sync_playwright() as playwright:
            browser = playwright.chromium.launch(headless=True)
            context = browser.new_context(ignore_https_errors=True)
            first = context.new_page()
            first.goto(origin, wait_until='networkidle')
            first.add_script_tag(content=bundle)
            first.evaluate('''async (accounts) => {
                const beneficiaries = [];
                for (const account of accounts) {
                  const response = await window.sessionTestClient.apiFetch('/api/v1/auth/register/beneficiary', {
                    method: 'POST', headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({displayName: 'Fictional Beneficiary', ...account, isFamily: false})
                  });
                  beneficiaries.push((await response.json()).beneficiaryId);
                }
                window.beneficiaryB = beneficiaries[1];
                await window.sessionTestClient.apiFetch('/api/v1/auth/login', {
                  method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(accounts[0])
                });
                window.accountA = await (await window.sessionTestClient.apiFetch('/api/v1/auth/me')).json();
            }''', accounts)
            second = context.new_page()
            second.goto(origin, wait_until='networkidle')
            second.add_script_tag(content=bundle)
            second.evaluate('''async (account) => {
                await window.sessionTestClient.apiFetch('/api/v1/auth/login', {
                  method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(account)
                });
                window.accountB = await (await window.sessionTestClient.apiFetch('/api/v1/auth/me')).json();
            }''', accounts[1])
            before = second.evaluate('''async () =>
                (await (await window.sessionTestClient.apiFetch('/api/v1/requests')).json()).length''')
            stale = first.evaluate('''async () => {
                const {apiFetch, withExpectedUser} = window.sessionTestClient;
                const capture = async (path, options) => {
                  try { await apiFetch(path, withExpectedUser({status: 'authenticated', user: window.accountA}, options));
                        return {status: 200, dataReturned: true}; }
                  catch (error) { return {status: error.status, dataReturned: false}; }
                };
                const read = await capture('/api/v1/requests', {method: 'GET'});
                const write = await capture('/api/v1/requests', {
                  method: 'POST', headers: {'Content-Type': 'application/json'},
                  body: JSON.stringify({beneficiaryId: window.beneficiaryB, description: 'Fictional request',
                                        authorityConfirmed: true, sharingAgreed: true, needs: []})
                });
                const logout = await capture('/api/v1/auth/logout', {method: 'POST'});
                return {read, write, logout};
            }''')
            after = second.evaluate('''async () => {
                const response = await window.sessionTestClient.apiFetch('/api/v1/auth/me');
                const sameUser = (await response.json()).id === window.accountB.id;
                const requests = await (await window.sessionTestClient.apiFetch('/api/v1/requests')).json();
                return {sameUser, count: requests.length};
            }''')
            blocked = {'status': 409, 'dataReturned': False}
            self.assertEqual(stale, {'read': blocked, 'write': blocked, 'logout': blocked})
            self.assertTrue(after['sameUser'])
            self.assertEqual(after['count'], before)
            context.close()
            browser.close()


if __name__ == '__main__':
    unittest.main()
