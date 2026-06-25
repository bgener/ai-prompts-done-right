// Renders the real summarize-weather.prompty for promptfoo, so the eval has no
// second copy of the prompt. The .prompty file stays the single source of truth.
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";

const promptyPath = resolve(
  dirname(fileURLToPath(import.meta.url)),
  "../../src/Weather.Mcp/Prompts/summarize-weather.prompty",
);

// .prompty is YAML frontmatter, then role sections ("system:", "user:") whose
// bodies use {{var}} placeholders. Drop the frontmatter and split on the roles.
function parsePrompty(text) {
  const body = text.replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n/, "");
  const parts = body.split(/^(system|user|assistant):[ \t]*$/m);
  const messages = [];
  for (let i = 1; i < parts.length; i += 2) {
    messages.push({ role: parts[i], content: parts[i + 1].trim() });
  }
  return messages;
}

export default function render({ vars }) {
  const messages = parsePrompty(readFileSync(promptyPath, "utf8"));
  return messages.map((m) => ({
    role: m.role,
    content: m.content.replace(/\{\{\s*(\w+)\s*\}\}/g, (_, key) => vars[key] ?? ""),
  }));
}
