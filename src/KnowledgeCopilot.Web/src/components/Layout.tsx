import { Link, NavLink, Outlet } from 'react-router';
import logo from '../assets/logo.png';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-lg px-3 py-1.5 ${
    isActive
      ? 'bg-(--accent-bg) font-semibold text-(--accent)'
      : 'hover:text-(--text-h)'
  }`;

export const Layout = () => (
  <>
    <header className="flex flex-col items-center gap-4 px-4 pt-6">
      <Link to="/">
        <img
          src={logo}
          alt="KnowledgeCopilot"
          className="w-full max-w-[21.6rem] rounded-2xl shadow-lg"
        />
      </Link>
      <nav className="flex flex-wrap justify-center gap-2">
        <NavLink to="/" end className={navLinkClass}>
          Ask
        </NavLink>
        <NavLink to="/documents" end className={navLinkClass}>
          Documents
        </NavLink>
        <NavLink to="/documents/upload" className={navLinkClass}>
          Upload
        </NavLink>
      </nav>
    </header>
    <main className="flex-1">
      <Outlet />
    </main>
    <footer className="mt-auto border-t border-(--border) px-4 py-4 text-center text-sm">
      Built by{' '}
      <a
        href="https://www.linkedin.com/in/jasmin-krhan-031a84b1/"
        target="_blank"
        rel="noopener noreferrer"
        className="text-(--accent) underline"
      >
        Jasmin Krhan
      </a>
    </footer>
  </>
);
