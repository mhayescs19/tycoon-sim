---
name: feature-spec
description: Kicks off a new feature by finding the next incomplete phase in specs/roadmap.md, creating a git branch, interviewing the user about scope/decisions/context, and writing a dated spec directory under specs/ containing plan.md, requirements.md, and validation.md. Trigger when the user says "feature spec", "next phase", "start the next feature", or invokes /feature-spec.
---

# Feature Spec

## Workflow

### 1. Interview the user — BEFORE writing any files

Use `AskUserQuestion` with exactly **3 questions in one call**:

| Header | Question focus |
|--------|---------------|
| **Scope** | What the feature collects, exposes, or does — fields, behaviour, data shape |
| **Decisions** | Key implementation choices — storage, visibility, validation, UX pattern |
| **Context** | Tone, constraints, or anything shaping the spec — copy style, stack limits, open questions |

Do **not** write any files until the user has answered all three questions.

### 2. Read guidance files

Read `specs/mission.md` and `specs/tech-stack.md` before drafting.

### 3. Create the spec directory

Name: `specs/YYYY-MM-DD-<feature-name>/` using today's date.

#### `requirements.md`
- Scope section: what is and is not included; field/data table if applicable
- Decisions section: choices made and why (draw from user answers)
- Context section: tone rules, stack pointers, existing patterns to follow

#### `plan.md`
- Numbered task groups appropriate to the feature (for example: Data → Components → Page & Route → Navigation → Tests)
- Each group has numbered sub-tasks; groups should be independently implementable

#### `validation.md`
- Automated: project test and typecheck commands pass; specific assertions required
- Manual: walkthrough, behaviour, edge cases
- Tone check if the feature has user-facing copy
- Definition of done

## Constraints

- Respect the existing tech stack defined in `docs/specs/tech-stack.md` — no new dependencies without user approval. Create `tech-stack.md` if not exists
- Abide by the feature set defined in `doc/specs/project-spec.md`
- Follow existing conventions and patterns already established in the codebase
- Keep feature scope focused and independently shippable