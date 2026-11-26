import { useState } from 'react';
import { CatalogCategoryPanel } from './CatalogCategoryPanel';
import { CatalogServicePanel } from './CatalogServicePanel';
import { CatalogStandardsPanel } from './CatalogStandardsPanel';
import { CatalogHistoryPanel } from './CatalogHistoryPanel';
import { CatalogVersionPanel } from './CatalogVersionPanel';

export function CoordinatorStandardsWorkspace({ request, user }) {
  const [category, setCategory] = useState(null);
  const [service, setService] = useState(null);
  const [standard, setStandard] = useState(null);
  const [version, setVersion] = useState(null);
  return (
    <main className="catalog-page">
      <header className="catalog-topbar"><a href="/" className="catalog-brand">Helping Hands</a><span>Signed in as {user.displayName}</span></header>
      <section className="catalog-content">
        <p className="catalog-eyebrow">Coordinator workspace</p><h1>Services and standards</h1>
        <p className="catalog-lead">Create service categories, define unit standards and keep earlier published versions available for history.</p>
        <div className="catalog-grid">
          <CatalogCategoryPanel request={request} selected={category?.id} onSelect={item => { setCategory(item); setService(null); setStandard(null); setVersion(null); }} />
          {category && <CatalogServicePanel key={category.id} request={request} category={category} selected={service?.id} onSelect={item => { setService(item); setStandard(null); setVersion(null); }} />}
          {service && <CatalogStandardsPanel key={service.id} request={request} service={service} selected={standard?.id} onSelect={item => { setStandard(item); setVersion(null); }} />}
          {standard && <CatalogHistoryPanel key={standard.id} request={request} standard={standard} selected={version?.id} onSelect={setVersion} />}
          {version && <CatalogVersionPanel key={version.id} request={request} versionId={version.id} />}
        </div>
        <p className="catalog-footnote">Published service and standard versions are read-only. Create a new version to change what future requests can select; existing records keep their original version.</p>
      </section>
    </main>
  );
}
