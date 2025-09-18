
import type { Comment } from "@/services/schemas"
export default function CommentsComponent({ comments = [] }: { comments: Comment[] }) {
  return (
    <>
      <h3 className="text-lg font-semibold">Comments</h3>
      {comments.length ? (
        <div className="space-y-3 border rounded-md p-4">
          {comments.map((c: Comment) => (
            <div key={c.CommentId} className="border rounded-md p-3 text-sm space-y-1">
              <p className="whitespace-pre-wrap">{c.CommentText}</p>
              <div className="flex gap-4 text-xs text-muted-foreground">
                <span>{c.CommentType}</span>
                <span>{c.UserCreated}</span>
                <span>{new Date(c.CreationDateTime).toLocaleString()}</span>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-muted-foreground italic">No comments.</p>
      )}
    </>
  )
};
