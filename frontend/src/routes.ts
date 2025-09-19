import { type RouteConfig, index, prefix, route } from "@react-router/dev/routes";

export default [
  // * matches all URLs , the ? makes it optional so it will match / as well
  // Root path '/' shows a home/landing page
  index("./pages/HomePage.tsx"),

  ...prefix("ground-truths", [
    index("./pages/GroundTruthListPage.tsx"),
    route(":id", "./pages/GroundTruthDetailLayout.tsx", [
      index("./pages/GroundTruthDetailView.tsx"),
      route("edit", "./pages/GroundTruthEditForm.tsx"),
    ]),
    // Action-only route for contexts updates (POST/PUT). Returns 404 on direct navigation.
    route(":id/contexts", "./routes/actions/updateGroundTruthContexts.ts"),
    route(":id/queries", "./routes/actions/updateGTQueries.ts")
  ]),
  route("/login", "pages/LoginPage.tsx"),
  route("*?", "catchall.tsx"),

] satisfies RouteConfig;
