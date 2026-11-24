import { Link, Outlet } from 'react-router';
import logo from '../assets/logo.png';

export const Layout = () => (
  <>
    <header className="flex justify-center pt-6">
      <Link to="/">
        <img
          src={logo}
          alt="KnowledgeCopilot"
          className="w-full max-w-[21.6rem] rounded-2xl shadow-lg"
        />
      </Link>
    </header>
    <main>
      <Outlet />
    </main>
  </>
);
