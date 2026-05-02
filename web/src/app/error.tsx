"use client";

type ErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function Error({ error, reset }: ErrorProps) {
  return (
    <div className="p-6">
      <h2 className="text-lg font-semibold">Something went wrong</h2>
      <p className="text-sm text-red-700 mt-2">{error.message}</p>
      <button
        type="button"
        onClick={reset}
        className="mt-4 px-3 py-1 rounded border hover:bg-gray-100"
      >
        Try again
      </button>
    </div>
  );
}
