import { updateDataQueries } from "../../services/groundTruthService";
import { DataQueryDefinitionDtoArraySchema } from "../../types/schemas";
import type { Route } from "./+types/root";

export async function clientAction({ request, params }: Route.ClientActionArgs) {
  const method = request.method.toUpperCase();
  if (!["POST", "PUT"].includes(method)) {
    return new Response("Method Not Allowed", { status: 405 });
  }


  const groundTruthId = params.id;
  if (!groundTruthId) {
    return new Response("Missing Ground Truth Id", { status: 400 });
  }
  try {
    const data = await request.formData();
    const raw = data.get("queries");
    console.log({ raw })
    if (typeof raw !== "string") {
      return new Response("Missing contexts payload", { status: 400 });
    }
    let parsedJson: unknown;
    try {
      parsedJson = JSON.parse(raw);
    } catch {
      return new Response("Invalid JSON in contexts field", { status: 400 });
    }

    const queries = DataQueryDefinitionDtoArraySchema.parse(parsedJson);
    // ! HOTFIX for backend enum mismatch - backend expects 0/1 not "Sql / cosmosDb"
    const mapped = queries.map(q => ({
      ...q,
      DatastoreType: q.DatastoreType === 'Sql' ? 0 : 1
    }));

    const updated = await updateDataQueries(groundTruthId, mapped as any);
    return new Response(JSON.stringify({ groundTruth: updated }), {
      status: 200,
      headers: { "Content-Type": "application/json" }
    });
  } catch (err: any) {
    console.error("updateGTQueries action error", err);
    const status = typeof err === "object" && err && "status" in err ? (err as any).status || 500 : 500;
    return new Response(
      JSON.stringify({ error: err?.message || "Failed to update queries" }),
      { status, headers: { "Content-Type": "application/json" } }
    );
  }

}
