import { Link } from 'react-router';

export const NotFoundPage = () => (
  <section className="px-4 py-12 text-center">
    <h1 className="mb-2 text-2xl font-semibold text-(--text-h)">
      Page not found
    </h1>
    <p className="mb-6">The page you are looking for doesn't exist.</p>
    <Link to="/" className="text-(--accent) underline">
      Back to home
    </Link>
  </section>
);
