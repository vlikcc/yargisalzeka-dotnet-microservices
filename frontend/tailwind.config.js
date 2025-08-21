/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0F2748',
          foreground: '#FFFFFF'
        },
        secondary: {
          DEFAULT: '#C49A3D',
          foreground: '#FFFFFF'
        }
      }
    }
  },
  plugins: [],
};
