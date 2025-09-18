// Loader for ground truth detail route (Phase 1 Task 1.2)
// Fetches a single ground truth definition by id.
import { getGroundTruthDefinition } from "@/services/groundTruthService";

interface LoaderArgs {
  params: { id?: string };
}

export async function groundTruthDetailLoader({ params }: LoaderArgs) {
  const id = params.id?.trim();
  if (!id) {
    throw new Response("Missing id", { status: 400 });
  }
  try {
    const groundTruth = await getGroundTruthDefinition(id);
    console.log({ groundTruth })
    return { groundTruth };
  } catch (err: any) {
    if (err && typeof err === 'object' && 'status' in err && (err as any).status === 404) {
      throw new Response("Not Found", { status: 404 });
    }
    throw err;
  }
}
