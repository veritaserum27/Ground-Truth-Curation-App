import { Clock, Star } from "lucide-react";
import { useState } from "react";
import type { GroundTruthDefinition, GroundTruthEntry } from "~/services/schemas";

export default function GeneratedResponse({ groundTruth }: { groundTruth: GroundTruthDefinition }) {
  const [activeResponseTab, setActiveResponseTab] = useState<Record<string, 'formatted' | 'raw' | 'metadata'>>({});
  const filteredEntries: GroundTruthEntry[] = groundTruth.GroundTruthEntries.filter(entry => entry.Response && entry.Response.trim() !== '');
  return (
    <div className="space-y-4 mt-4">
      <h3 className="text-lg font-semibold">Generated Responses</h3>
      {filteredEntries.length > 0 ? (
        <div className="space-y-4 border rounded-md p-4">

          {filteredEntries.map(({ GroundTruthId, Response, CreationDateTime, RawData }, index) => (
            <div key={GroundTruthId} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <span>Response {index + 1}</span>
                  <span className="px-2 py-1 bg-purple-100 text-purple-800 rounded text-sm">
                    "ChatGPT-4 - PlaceHolder"
                  </span>
                  <span className="flex items-center gap-1 text-sm text-muted-foreground">
                    <Star className="w-4 h-4" />
                    Confidence: 0.95 - Placeholder
                  </span>
                </div>
                <span className="flex items-center gap-1 text-sm text-muted-foreground">
                  <Clock className="w-4 h-4" />
                  {new Date(CreationDateTime).toLocaleDateString()}
                </span>
              </div>

              <div className="space-y-3">
                <div className="flex gap-2 border-b">
                  {(["formatted", "raw", "metadata"] as const).map((tab) => (
                    <button
                      key={tab}
                      onClick={() =>
                        setActiveResponseTab((prev) => ({
                          ...prev,
                          [GroundTruthId]: tab,
                        }))
                      }
                      className={`px-3 py-2 text-sm border-b-2 ${(activeResponseTab[GroundTruthId] || "formatted") === tab
                        ? "border-primary text-primary"
                        : "border-transparent text-muted-foreground hover:text-foreground"
                        }`}
                    >
                      {tab.charAt(0).toUpperCase() + tab.slice(1)}
                    </button>
                  ))}
                </div>

                <div>
                  {(activeResponseTab[GroundTruthId] || "formatted") === "formatted" && (
                    <div className="bg-muted p-3 rounded">
                      {Response}
                    </div>
                  )}
                  {activeResponseTab[GroundTruthId] === "raw" && (
                    <pre className="bg-muted p-3 rounded text-sm whitespace-pre-wrap">
                      {JSON.stringify(RawData, null, 2)}
                    </pre>
                  )}
                  {activeResponseTab[GroundTruthId] === "metadata" && (
                    <div className="bg-muted p-3 rounded space-y-2">
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Model:</span>
                        <span>GPT-4</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Confidence:</span>
                        <span>0.92</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Timestamp:</span>
                        <span>{new Date(CreationDateTime).toLocaleString()}</span>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))
          }</div>
      ) : (
        <p className="text-muted-foreground text-center py-4">
          No generated responses yet.
        </p>
      )}
    </div>

  )
}
