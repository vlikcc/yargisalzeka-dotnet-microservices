import { NavLink } from 'react-router-dom';

const links = [
  { to: '/', label: 'Dashboard' },
  { to: '/search', label: 'Arama' },
  { to: '/petitions', label: 'Dilekçeler' },
  { to: '/subscription', label: 'Abonelik' },
  { to: '/history', label: 'Geçmiş' },
  { to: '/profile', label: 'Profil' },
];

export function Sidebar() {
  return (
    <aside className="w-56 border-r bg-white hidden md:flex flex-col">
      <div className="h-14 border-b flex items-center px-4 font-semibold">Menü</div>
      <nav className="flex-1 p-2 space-y-1">
        {links.map(l => (
          <NavLink
            key={l.to}
            to={l.to}
            end={l.to === '/'}
            className={({ isActive }) =>
              `block py-2 px-3 rounded text-sm font-medium hover:bg-gray-100 ${isActive ? 'bg-primary text-white hover:bg-primary' : 'text-gray-700'}`
            }
          >
            {l.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
