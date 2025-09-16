import { type RouteConfig, index, prefix, route } from "@react-router/dev/routes";

export default [
  // * matches all URLs , the ? makes it optional so it will match / as well
  // Root path '/' shows a home/landing page
  index("./pages/HomePage.tsx"),

  ...prefix("ground-truths", [
    index("./pages/GroundTruthListPage.tsx"),
    route(":id", "./pages/GroundTruthDetailPage.tsx"),
  ]),
  route("/login", "pages/LoginPage.tsx"),
  route("*?", "catchall.tsx"),

] satisfies RouteConfig;
