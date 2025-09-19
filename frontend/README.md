
  # Ground Truth Q&A Application

  This is a code bundle for Ground Truth Q&A Application. The original project is available at https://www.figma.com/design/6Eco7jB0eBtxb5pKQ3f389/Ground-Truth-Q-A-Application.

  ## Running the code

  Preferred package manager: pnpm (lockfile present).

  ```sh
  pnpm install
  pnpm dev
  ```

  Or with npm:

  ```sh
  npm install
  npm run dev
  ```

  ## Environment Variables

  Copy `.env.example` to `.env` and adjust. Only variables prefixed with `VITE_` are exposed to the client.

  - `VITE_API_BASE_URL` (default fallback: http://localhost:5000)

  ## Ground Truth Service

  The file `src/services/groundTruthService.ts` provides typed access to backend API endpoints:

  | Method | Purpose | Status |
  | ------ | ------- | ------ |
  | `listGroundTruthDefinitions(filter?)` | Fetch all definitions (optional userId, validationStatus filter) | Implemented |
  | `getGroundTruthDefinition(id)` | Fetch a single definition by ID | Implemented |
  | `createGroundTruthDefinition(payload)` | Create new definition | Placeholder (501) |
  | `addGroundTruthEntry(payload)` | Add entry to definition | Placeholder (501) |
  | `updateValidationStatus(payload)` | Update validation status | Placeholder (501) |

  ### Example Usage

  ```ts
  import { groundTruthService } from '@/services/groundTruthService';

  async function load() {
    try {
      const defs = await groundTruthService.listGroundTruthDefinitions({ validationStatus: 'validated' });
      console.log(defs);
    } catch (e) {
      console.error(e);
    }
  }
  ```

  ### Mapping Notes
  Backend entities use PascalCase; the service maps them to existing UI models, filling placeholder values (e.g., `category` always `asset_knowledge` for now). When the backend exposes richer domain fields, update the mapper instead of scattered UI changes.

  ## Future Enhancements
  - Integrate API data into `DataContext` replacing mock dataset.
  - Implement create/update endpoints once backend adds functionality.
  - Add caching and stale-while-revalidate layer.
  - Add tests for mapping and error handling.
