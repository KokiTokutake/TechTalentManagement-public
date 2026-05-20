import type { Config } from 'tailwindcss'

export default {
  content: [
    './components/**/*.{js,vue,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './plugins/**/*.{js,ts}',
    './app.vue',
  ],
  theme: {
    extend: {
      colors: {
        canvas: '#f7f4f0',
        surface: '#ffffff',
        'surface-muted': '#f0ede8',
        border: '#e2ddd4',
        'border-strong': '#ccc6bb',
        accent: {
          DEFAULT: '#c05621',
          hover: '#a04818',
          light: '#fef0e7',
        },
        nav: {
          bg: '#1c1814',
          text: '#e2ddd4',
          muted: '#9c948a',
          border: '#2d2824',
          'active-bg': 'rgba(192, 86, 33, 0.15)',
          'active-text': '#f07b3a',
        },
        ink: {
          DEFAULT: '#1a1714',
          secondary: '#6b6259',
          muted: '#9c948a',
        },
      },
      fontFamily: {
        heading: ['Noto Serif JP', 'Yu Mincho', 'Georgia', 'serif'],
        body: ['Noto Sans JP', 'Yu Gothic UI', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'Cascadia Code', 'Fira Code', 'monospace'],
      },
      borderRadius: {
        sm: '4px',
        DEFAULT: '8px',
        lg: '12px',
        xl: '16px',
      },
    },
  },
  plugins: [],
} satisfies Config
