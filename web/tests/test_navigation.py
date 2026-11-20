import os
import unittest
from playwright.sync_api import sync_playwright


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
        self.assertTrue(page.get_by_role('heading', name='How it works').is_visible())
        page.reload(wait_until='networkidle')
        self.assertTrue(page.get_by_text('A person makes a request').is_visible())
        page.go_back(wait_until='networkidle')
        self.assertTrue(page.get_by_role('heading', name='Coordinate requests for assistance.').is_visible())
        page.close()

    def test_mobile_layout_and_keyboard_navigation(self):
        page = self.browser.new_page(viewport={'width': 375, 'height': 812})
        page.goto(self.base, wait_until='networkidle')
        self.assertLessEqual(page.evaluate('document.documentElement.scrollWidth'), 375)
        page.keyboard.press('Tab')
        self.assertEqual(page.evaluate('document.activeElement.getAttribute("aria-label")'), 'Helping Hands home')
        page.keyboard.press('Tab')
        page.keyboard.press('Enter')
        self.assertTrue(page.get_by_role('heading', name='How it works').is_visible())
        page.close()


if __name__ == '__main__':
    unittest.main()
