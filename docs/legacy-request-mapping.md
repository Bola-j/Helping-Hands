# Legacy request service mapping

Before a new referral can be created for a pre-versioned legacy request, an authenticated Coordinator or Administrator maps it once using `POST /api/v1/requests/{id}/legacy-standard` with the currently observed request `version` and a published, effective `standardVersionId`.

The request response exposes `legacyStandardVersionId` and `legacyMappedAt`. The mapping records the actor and time, is immutable, and determines which recipient service and Cairo-day capacity the referral must match. Existing referrals are not backfilled or changed. Multi-need requests use their need-level catalog selections and are not eligible for this legacy mapping route.
