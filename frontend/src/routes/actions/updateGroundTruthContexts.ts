// Action route for updating GroundTruth contexts only.
// Uses existing service updateGroundTruthContext to persist changes.
// This file intentionally has no default component export; it's an action-only route.
import { updateGroundTruthContext } from "../../services/groundTruthService";
import { GroundTruthContextDtoArraySchema } from "../../types/schemas";
import type { Route } from "./+types/root";


export async function clientAction({ request, params }: Route.ClientActionArgs) {
  const method = request.method.toUpperCase();
  if (!["POST", "PUT"].includes(method)) {
    return new Response("Method Not Allowed", { status: 405 });
  }
  const groundTruthId = params.id;
  if (!groundTruthId) {
    return new Response("Missing ground truth id", { status: 400 });
  }

  try {
    const formData = await request.formData();
    const raw = formData.get("contexts");
    const parsed = JSON.parse(raw);
    console.log({ parsed })
    if (typeof raw !== "string") {
      return new Response("Missing contexts payload", { status: 400 });
    }
    let parsedJson: unknown;
    try {
      parsedJson = JSON.parse(raw);
    } catch {
      return new Response("Invalid JSON in contexts field", { status: 400 });
    }

    // Validate shape (lenient: allow extra fields, strip unknown by re-parsing after stringifying validated value)
    const contexts = GroundTruthContextDtoArraySchema.parse(parsedJson);
    console.log({ contexts })
    const updated = await updateGroundTruthContext(groundTruthId, contexts);
    return new Response(JSON.stringify({ groundTruth: updated }), {
      status: 200,
      headers: { "Content-Type": "application/json" }
    });
  } catch (err: any) {
    console.error("updateGroundTruthContexts action error", err);
    const status = typeof err === "object" && err && "status" in err ? (err as any).status || 500 : 500;
    return new Response(
      JSON.stringify({ error: err?.message || "Failed to update contexts" }),
      { status, headers: { "Content-Type": "application/json" } }
    );
  }
}

// Optionally export a loader returning 404 for direct GET navigations to avoid silent blank pages.
export async function clientLoader() {
  return new Response("Not Found", { status: 404 });
}
