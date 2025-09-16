
  # Ground Truth Q&A Application

  This is a code bundle for Ground Truth Q&A Application. The original project is available at https://www.figma.com/design/6Eco7jB0eBtxb5pKQ3f389/Ground-Truth-Q-A-Application.

  ## Running the code

  Using pnpm (recommended):

  ```sh
  pnpm install
  pnpm dev
  ```

  Or with npm:

  ```sh
  npm install
  npm run dev
  ```

  ## Environment Variable
  - `VITE_API_BASE_URL` (default http://localhost:5000)

  ## Ground Truth Service
  The service in `src/services/groundTruthService.ts` interfaces with the backend:

  | Method | Description | Notes |
  | ------ | ----------- | ----- |
  | `listGroundTruthDefinitions(filter?)` | Fetch array of definitions | Validated & mapped |
  | `getGroundTruthDefinition(id)` | Fetch single definition | Validated & mapped |
  | `createGroundTruthDefinition` | Placeholder (501) | Backend not implemented |
  | `addGroundTruthEntry` | Placeholder (501) | Backend not implemented |
  | `updateValidationStatus` | Placeholder (501) | Backend not implemented |

  ### Validation
  Responses are validated with Zod (`schemas.ts`). Invalid payloads raise an `ApiError` with `status=422` and `issues` describing schema failures.

  ### Example
  ```ts
  import { groundTruthService } from '~/services/groundTruthService';

  async function loadDefs() {
    try {
      const defs = await groundTruthService.listGroundTruthDefinitions({ validationStatus: 'validated' });
      console.log(defs);
    } catch (e: any) {
      if (e.status === 422) console.error('Schema issues', e.issues);
    }
  }
  ```

  ### Mapping Notes
  - Backend PascalCase -> frontend camelCase performed in mapper functions.
  - Placeholder values (category, contexts, etc.) will evolve as backend adds richer fields.
