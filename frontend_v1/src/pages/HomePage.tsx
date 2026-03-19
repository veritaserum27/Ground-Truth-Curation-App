import { NavLink } from 'react-router';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '../components/ui/card';

export default function HomePage() {
  return (
    <main className="mx-auto flex max-w-3xl flex-col gap-8 px-6 py-12">
      <section className="space-y-4 text-center">
        <h1 className="text-3xl font-bold tracking-tight">Ground Truth Curation Hub</h1>
        <p className="text-muted-foreground text-base leading-relaxed">
          Curate, validate, and manage high quality ground truth knowledge used to evaluate and
          improve AI systems. Start by exploring existing entries or curate new multi-source
          definitions with structured parameters, tags, and validation workflows.
        </p>
      </section>

      <Card>
        <CardHeader>
          <CardTitle className="text-xl font-semibold">Get Started</CardTitle>
          <CardDescription>
            Jump directly into the curated ground truths list to review, validate, or export data.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3 text-sm leading-relaxed">
          <p>
            The application supports role-based workflows (Curator / Validator), tagging, parameterized
            contexts, multi-source query representations, and export to JSONL or CSV.
          </p>
          <ul className="list-disc space-y-1 pl-5 text-left">
            <li>Filter by tag, category, or validation status</li>
            <li>Drill into entries to inspect raw query data and formatted responses</li>
            <li>Track revisions with timestamps and reviewer feedback</li>
          </ul>
        </CardContent>
        <CardFooter>
          <Button asChild size="lg">
            <NavLink to="/ground-truths">View Ground Truths</NavLink>
          </Button>
        </CardFooter>
      </Card>

      <div className="text-center text-xs text-muted-foreground">
        Future enhancements: search, API-backed persistence, authentication integration, dashboards.
      </div>
    </main>
  );
}
