"""Browser-level client/API contract scenarios using fictional, isolated API responses.

The tests exercise the built React app through Playwright and a deterministic HTTP
boundary. Backend authorization and antiforgery enforcement are covered separately
by the API integration suite.
"""
import json
import os
import unittest
from urllib.parse import urlsplit

from playwright.sync_api import expect, sync_playwright


BENEFICIARY = {
    "id": "11111111-1111-4111-8111-111111111111", "displayName": "Fictional Beneficiary",
    "email": "beneficiary@example.invalid", "roles": ["Beneficiary"],
}
CHARITY = {
    "id": "22222222-2222-4222-8222-222222222222", "displayName": "Fictional Representative",
    "email": "charity@example.invalid", "roles": ["RecipientRepresentative"],
}
COORDINATOR = {
    "id": "33333333-3333-4333-8333-333333333333", "displayName": "Fictional Coordinator",
    "email": "coordinator@example.invalid", "roles": ["Coordinator"],
}
REQUEST_ID = "44444444-4444-4444-8444-444444444444"
NEED_ID = "55555555-5555-4555-8555-555555555555"
OUTCOME_ID = "66666666-6666-4666-8666-666666666666"
VERSION = "77777777-7777-4777-8777-777777777777"
EVIDENCE_ID = "88888888-8888-4888-8888-888888888888"


class BrowserApi:
    def __init__(self, user=None):
        self.user = user
        self.login_users = {item["email"]: item for item in (BENEFICIARY, CHARITY, COORDINATOR)}
        self.calls = []
        self.unavailable = set()
        self.forbid_request_list = False
        self.outcome_status = "Submitted"
        self.request_status = "Open"
        self.history = []

    def handle(self, route):
        request = route.request
        path = urlsplit(request.url).path
        call = {"method": request.method, "path": path, "headers": request.headers,
                "body": json.loads(request.post_data or "null") if request.method == "POST" else None}
        self.calls.append(call)
        if path in self.unavailable:
            route.abort("failed")
        elif request.method == "GET" and path == "/api/v1/auth/me":
            self.reply(route, 200, self.user) if self.user else self.reply(route, 401, {"title": "Unauthorized"})
        elif request.method == "GET" and path == "/api/v1/auth/antiforgery":
            route.fulfill(status=200, json={"requestToken": "fictional-csrf-token", "headerName": "X-CSRF-TOKEN"},
                          headers={"Set-Cookie": "hh-csrf=fictional-cookie; Path=/; SameSite=Strict"})
        elif request.method == "POST" and path == "/api/v1/auth/login":
            self.user = self.login_users.get(call["body"].get("email"), COORDINATOR)
            route.fulfill(status=204, headers={"Set-Cookie": "hh-session=fictional; Path=/; HttpOnly; SameSite=Strict"})
        elif request.method == "POST" and path == "/api/v1/auth/logout":
            self.user = None
            route.fulfill(status=204)
        elif request.method == "GET" and path == "/api/v1/beneficiaries":
            self.reply(route, 200, [{"id": BENEFICIARY["id"], "reference": "BEN-DEMO", "displayName": BENEFICIARY["displayName"], "isFamily": False}])
        elif request.method == "GET" and path == "/api/v1/catalog/current":
            self.reply(route, 200, {"items": [{"standardVersionId": "99999999-9999-4999-8999-999999999999",
                "categoryCode": "GOODS", "serviceName": "Fictional food parcel", "standardName": "Weekly parcel",
                "standardVersionNumber": 1, "serviceVersionNumber": 1, "components": []}], "nextCursor": None})
        elif request.method == "POST" and path == "/api/v1/requests":
            self.reply(route, 201, {"reference": "REQ-DEMO-001", "status": "Submitted"})
        elif request.method == "GET" and path == "/api/v1/requests":
            if self.forbid_request_list:
                self.reply(route, 403, {"title": "Forbidden"})
            else:
                self.reply(route, 200, [{"id": REQUEST_ID, "reference": "REQ-DEMO-001", "status": self.request_status,
                    "needs": [{"id": NEED_ID}]}])
        elif request.method == "GET" and path == f"/api/v1/requests/{REQUEST_ID}":
            self.reply(route, 200, {"id": REQUEST_ID, "reference": "REQ-DEMO-001", "isMultiNeed": True,
                "description": "Fictional food support", "status": self.request_status, "version": VERSION,
                "needs": [{"id": NEED_ID, "serviceName": "Fictional food parcel", "status": "Accepted", "quantity": 1}]})
        elif request.method == "GET" and path == f"/api/v1/requests/{REQUEST_ID}/progress":
            accepted = 1 if self.outcome_status == "Accepted" else 0
            self.reply(route, 200, {"id": REQUEST_ID, "reference": "REQ-DEMO-001", "status": self.request_status,
                "description": "Fictional food support", "history": [], "needs": [{
                    "needId": NEED_ID, "service": "Fictional food parcel", "standard": "Weekly parcel",
                    "requiredQuantity": 1, "status": "Accepted", "assignedQuantity": 1,
                    "acceptedQuantity": accepted, "remainingQuantity": 1 - accepted,
                    "assignments": [{"id": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa", "reference": "REF-DEMO-001",
                        "scopeDescription": "Fictional weekly parcel", "assignedQuantity": 1,
                        "acceptedQuantity": accepted, "status": "Assigned", "outcomes": [{
                            "id": OUTCOME_ID, "reference": "OUT-DEMO-001", "reportedQuantity": 1,
                            "description": "Fictional delivery report", "status": self.outcome_status,
                            "submittedAt": "2026-11-27T10:00:00Z", "version": VERSION,
                            "evidence": [{"id": EVIDENCE_ID, "originalName": "fictional receipt.pdf",
                                "mediaType": "application/pdf", "sizeBytes": 4096, "uploadedAt": "2026-11-27T10:00:00Z"}],
                        }]}],
                }]})
        elif request.method == "GET" and path == f"/api/v1/requests/{REQUEST_ID}/history":
            self.reply(route, 200, self.history)
        elif request.method == "POST" and path == f"/api/v1/outcomes/{OUTCOME_ID}/reviews":
            self.outcome_status = call["body"]["decision"]
            self.reply(route, 200, {"id": OUTCOME_ID, "reference": "OUT-DEMO-001", "status": self.outcome_status})
        elif request.method == "POST" and path == f"/api/v1/requests/{REQUEST_ID}/closure":
            self.request_status = "Completed"
            self.history.append({"action": "RequestClosed", "reason": None, "occurredAt": "2026-11-27T11:00:00Z"})
            route.fulfill(status=204)
        else:
            self.reply(route, 404, {"title": "Not found"})

    @staticmethod
    def reply(route, status, body):
        route.fulfill(status=status, json=body)


class RoleWorkflowBrowserTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch(headless=True)
        cls.base = os.environ.get("WEB_TEST_URL", "http://localhost:8081")

    @classmethod
    def tearDownClass(cls):
        cls.browser.close()
        cls.playwright.stop()

    def page_for(self, api):
        context = self.browser.new_context()
        page = context.new_page()
        page.route("**/api/v1/**", api.handle)
        return context, page

    def test_login_uses_antiforgery_cookie_and_shows_role_links(self):
        api = BrowserApi()
        context, page = self.page_for(api)
        page.goto(self.base + "/login", wait_until="networkidle")
        page.get_by_label("Email").fill(COORDINATOR["email"])
        page.get_by_role("textbox", name="Password").fill("FictionalPassword1!")
        page.get_by_role("button", name="Sign in").click()
        expect(page.get_by_role("heading", name="You are signed in")).to_be_visible()
        expect(page.get_by_role("link", name="Follow-up and closure")).to_have_attribute("href", "/coordinator/follow-up")
        expect(page.get_by_role("link", name="Match and refer")).to_be_visible()
        login = next(item for item in api.calls if item["method"] == "POST" and item["path"] == "/api/v1/auth/login")
        self.assertEqual(login["headers"].get("x-csrf-token"), "fictional-csrf-token")
        self.assertIn("hh-csrf=fictional-cookie", login["headers"].get("cookie", ""))
        context.close()

    def test_beneficiary_form_validates_then_submits_through_csrf(self):
        api = BrowserApi(BENEFICIARY)
        context, page = self.page_for(api)
        page.goto(self.base + "/requests/new", wait_until="networkidle")
        expect(page.get_by_role("heading", name="Request support")).to_be_visible()
        page.get_by_role("button", name="Submit request").click()
        expect(page.get_by_role("alert")).to_contain_text("Describe the support you are asking for")
        self.assertFalse(any(item["method"] == "POST" and item["path"] == "/api/v1/requests" for item in api.calls))
        page.get_by_label("What support do you need?").fill("Fictional food support for one household.")
        page.get_by_label("Service and unit standard").select_option("99999999-9999-4999-8999-999999999999")
        page.get_by_label("I have permission to submit this request for the beneficiary.").check()
        page.get_by_label("I agree that the request details can be shared with coordinators and suitable charities.").check()
        page.get_by_role("button", name="Submit request").click()
        expect(page.get_by_role("heading", name="Request submitted")).to_be_visible()
        submitted = next(item for item in api.calls if item["method"] == "POST" and item["path"] == "/api/v1/requests")
        self.assertEqual(submitted["headers"].get("x-csrf-token"), "fictional-csrf-token")
        self.assertEqual(submitted["body"]["description"], "Fictional food support for one household.")
        self.assertTrue(submitted["body"]["authorityConfirmed"] and submitted["body"]["sharingAgreed"])
        context.close()

    def test_coordinator_reviews_evidence_and_closes_only_when_complete(self):
        api = BrowserApi(COORDINATOR)
        context, page = self.page_for(api)
        page.on("dialog", lambda dialog: dialog.accept())
        page.goto(self.base + "/coordinator/follow-up", wait_until="networkidle")
        expect(page.get_by_role("heading", name="Follow-up and closure")).to_be_visible()
        expect(page.get_by_role("link", name="fictional receipt.pdf")).to_be_visible()
        expect(page.get_by_text("1 quantity remains unfulfilled")).to_be_visible()
        self.assertFalse(any(item["path"].endswith("/closure") for item in api.calls))
        page.get_by_role("button", name="Accept reported quantity").click()
        expect(page.get_by_text("Ready to close")).to_be_visible()
        page.get_by_role("button", name="Close request").click()
        expect(page.get_by_text("Already completed")).to_be_visible()
        review = next(item for item in api.calls if item["method"] == "POST" and item["path"].endswith("/reviews"))
        close = next(item for item in api.calls if item["method"] == "POST" and item["path"].endswith("/closure"))
        self.assertEqual(review["body"]["decision"], "Accepted")
        self.assertEqual(close["body"]["version"], VERSION)
        self.assertEqual(close["headers"].get("x-csrf-token"), "fictional-csrf-token")
        self.assertTrue(any(item["action"] == "RequestClosed" for item in api.history))
        context.close()

    def test_role_specific_navigation_and_forbidden_workspace(self):
        scenarios = [
            (BENEFICIARY, "Make a support request", "Manage services and capacity"),
            (CHARITY, "Review assigned referrals", "Manage services and standards"),
            (COORDINATOR, "Follow-up and closure", "Make a support request"),
        ]
        for user, visible, hidden in scenarios:
            with self.subTest(role=user["roles"][0]):
                api = BrowserApi(user)
                context, page = self.page_for(api)
                page.goto(self.base + "/login", wait_until="networkidle")
                expect(page.get_by_role("heading", name="You are signed in")).to_be_visible()
                expect(page.get_by_role("link", name=visible)).to_be_visible()
                self.assertEqual(page.get_by_role("link", name=hidden).count(), 0)
                if user is BENEFICIARY:
                    page.goto(self.base + "/coordinator/standards", wait_until="networkidle")
                    expect(page.get_by_role("heading", name="Access not available")).to_be_visible()
                    self.assertFalse(any(item["path"] == "/api/v1/catalog/current" for item in api.calls))
                context.close()

    def test_unavailable_session_and_api_forbidden_states(self):
        unavailable = BrowserApi()
        unavailable.unavailable.add("/api/v1/auth/me")
        context, page = self.page_for(unavailable)
        page.goto(self.base + "/coordinator/standards", wait_until="networkidle")
        expect(page.get_by_role("alert")).to_contain_text("session service is unavailable")
        context.close()

        denied = BrowserApi(COORDINATOR)
        denied.forbid_request_list = True
        context, page = self.page_for(denied)
        page.goto(self.base + "/coordinator/requests", wait_until="networkidle")
        expect(page.get_by_role("alert")).to_contain_text("Coordinator or Administrator access is required")
        context.close()

    def test_signup_validation_does_not_send_invalid_credentials(self):
        api = BrowserApi()
        context, page = self.page_for(api)
        page.goto(self.base + "/register/beneficiary", wait_until="networkidle")
        page.get_by_label("Your name").fill("Fictional Person")
        page.get_by_label("Email").fill("not-an-email")
        page.get_by_role("textbox", name="Password").fill("short")
        page.get_by_role("button", name="Create account").click()
        expect(page.get_by_text("Enter an email address like name@example.org.")).to_be_visible()
        expect(page.get_by_text("Use at least 8 characters.")).to_be_visible()
        self.assertFalse(any(item["method"] == "POST" and item["path"].endswith("/register/beneficiary") for item in api.calls))
        context.close()


if __name__ == "__main__":
    unittest.main()
