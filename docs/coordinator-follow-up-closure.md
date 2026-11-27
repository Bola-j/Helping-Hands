# Coordinator follow-up and closure

The React browser workspace is available at `/coordinator/follow-up` to Coordinator and Administrator accounts. It lists requests and shows each need's requested, assigned, accepted and remaining quantities, its referrals, reported outcomes, evidence metadata, and request/decision history.

Outcome reports in `Submitted` status can be accepted, returned for clarification, or marked unresolved. Clarification and unresolved decisions require a reason. Evidence links use `GET /api/v1/attachments/{id}/content`; the API checks access and serves private files with `Cache-Control: no-store`. Only staff progress responses include evidence metadata. A report and its evidence are records for review, not independent proof that assistance was delivered.

The workspace shows why closure is blocked for needs that are not accepted or still have unmet quantity. When every recorded need is accepted and its accepted quantity meets the required quantity, the coordinator can explicitly close the request. The API remains authoritative: it checks the current request version and quantities, records `RequestClosed`, changes the status to `Completed`, and returns a conflict if the request changed or any requirement is unmet.

The UI is a browser-only React JavaScript page; it does not add PWA behavior. Its layout includes narrow-screen reflow, but issue #141 owns responsive acceptance and viewport verification. The implementation has not yet been manually rehearsed against a running local API. Automated tests were not run for this change; complete API/browser workflow evidence before closing issue #43.
