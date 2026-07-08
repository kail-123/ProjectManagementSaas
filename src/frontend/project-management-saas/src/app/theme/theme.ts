import { alpha, createTheme } from '@mui/material/styles';
import type { PaletteMode } from '@mui/material';

const neutral = {
  0: '#ffffff',
  25: '#fcfcfd',
  50: '#f8fafc',
  100: '#eef2f6',
  200: '#e2e8f0',
  300: '#cbd5e1',
  400: '#94a3b8',
  500: '#64748b',
  600: '#475569',
  700: '#334155',
  800: '#1e293b',
  900: '#0f172a',
  950: '#020617'
};

export const designTokens = {
  sidebarWidth: 288,
  sidebarCollapsedWidth: 76,
  topbarHeight: 72,
  radius: {
    xs: 8,
    sm: 10,
    md: 12,
    lg: 16,
    xl: 24
  },
  shadow: {
    sm: '0 1px 2px rgba(15, 23, 42, 0.06)',
    md: '0 14px 40px rgba(15, 23, 42, 0.08)',
    lg: '0 24px 70px rgba(15, 23, 42, 0.12)'
  }
};

export function createAppTheme(mode: PaletteMode) {
  const isDark = mode === 'dark';
  const palette = {
    mode,
    primary: {
      main: isDark ? '#8b5cf6' : '#4f46e5',
      light: isDark ? '#a78bfa' : '#818cf8',
      dark: isDark ? '#6d28d9' : '#3730a3',
      contrastText: '#ffffff'
    },
    secondary: {
      main: isDark ? '#22d3ee' : '#0891b2',
      light: isDark ? '#67e8f9' : '#22d3ee',
      dark: isDark ? '#0891b2' : '#0e7490',
      contrastText: isDark ? neutral[950] : '#ffffff'
    },
    success: {
      main: '#16a34a',
      light: '#86efac',
      dark: '#15803d'
    },
    warning: {
      main: '#d97706',
      light: '#fbbf24',
      dark: '#b45309'
    },
    error: {
      main: '#dc2626',
      light: '#fca5a5',
      dark: '#b91c1c'
    },
    info: {
      main: '#2563eb',
      light: '#93c5fd',
      dark: '#1d4ed8'
    },
    background: {
      default: isDark ? '#080b13' : '#f6f7fb',
      paper: isDark ? '#111827' : '#ffffff'
    },
    text: {
      primary: isDark ? '#f8fafc' : neutral[900],
      secondary: isDark ? '#94a3b8' : neutral[500]
    },
    divider: isDark ? alpha('#ffffff', 0.1) : '#e5e7eb'
  };

  return createTheme({
    palette,
    spacing: 8,
    shape: {
      borderRadius: designTokens.radius.md
    },
    shadows: [
      'none',
      designTokens.shadow.sm,
      designTokens.shadow.sm,
      designTokens.shadow.md,
      designTokens.shadow.md,
      designTokens.shadow.md,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg,
      designTokens.shadow.lg
    ],
    typography: {
      fontFamily: 'Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
      h1: { fontSize: '3rem', lineHeight: 1.08, fontWeight: 800, letterSpacing: 0 },
      h2: { fontSize: '2.25rem', lineHeight: 1.14, fontWeight: 800, letterSpacing: 0 },
      h3: { fontSize: '1.875rem', lineHeight: 1.2, fontWeight: 800, letterSpacing: 0 },
      h4: { fontSize: '1.5rem', lineHeight: 1.25, fontWeight: 780, letterSpacing: 0 },
      h5: { fontSize: '1.25rem', lineHeight: 1.32, fontWeight: 760, letterSpacing: 0 },
      h6: { fontSize: '1rem', lineHeight: 1.4, fontWeight: 740, letterSpacing: 0 },
      subtitle1: { fontSize: '0.9375rem', lineHeight: 1.5, fontWeight: 650, letterSpacing: 0 },
      subtitle2: { fontSize: '0.875rem', lineHeight: 1.45, fontWeight: 650, letterSpacing: 0 },
      body1: { fontSize: '0.9375rem', lineHeight: 1.6, letterSpacing: 0 },
      body2: { fontSize: '0.875rem', lineHeight: 1.55, letterSpacing: 0 },
      caption: { fontSize: '0.75rem', lineHeight: 1.45, letterSpacing: 0 },
      overline: { fontSize: '0.6875rem', lineHeight: 1.5, fontWeight: 750, letterSpacing: '0.08em', textTransform: 'uppercase' },
      button: { textTransform: 'none', fontWeight: 720, letterSpacing: 0 }
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage: isDark
              ? 'radial-gradient(circle at top left, rgba(79, 70, 229, 0.18), transparent 34rem)'
              : 'linear-gradient(180deg, #fbfcff 0%, #f6f7fb 100%)'
          },
          '::selection': {
            backgroundColor: alpha(isDark ? '#a78bfa' : '#4f46e5', 0.22)
          },
          '*:focus-visible': {
            outline: `3px solid ${alpha(isDark ? '#a78bfa' : '#4f46e5', 0.32)}`,
            outlineOffset: 2
          }
        }
      },
      MuiButton: {
        defaultProps: {
          disableElevation: true
        },
        styleOverrides: {
          root: {
            minHeight: 38,
            borderRadius: designTokens.radius.sm,
            paddingInline: 16,
            transition: 'transform 140ms ease, box-shadow 140ms ease, background-color 140ms ease',
            '&:hover': {
              transform: 'translateY(-1px)'
            }
          },
          contained: {
            boxShadow: `0 10px 24px ${alpha(isDark ? '#8b5cf6' : '#4f46e5', 0.24)}`,
            '&:hover': {
              boxShadow: `0 14px 32px ${alpha(isDark ? '#8b5cf6' : '#4f46e5', 0.3)}`
            }
          },
          outlined: {
            borderColor: isDark ? alpha('#ffffff', 0.14) : neutral[200],
            backgroundColor: isDark ? alpha('#ffffff', 0.03) : '#ffffff'
          }
        }
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            borderColor: isDark ? alpha('#ffffff', 0.1) : neutral[200]
          }
        }
      },
      MuiCard: {
        styleOverrides: {
          root: {
            border: `1px solid ${isDark ? alpha('#ffffff', 0.1) : neutral[200]}`,
            boxShadow: isDark ? 'none' : designTokens.shadow.sm,
            borderRadius: designTokens.radius.lg
          }
        }
      },
      MuiTextField: {
        defaultProps: {
          fullWidth: true,
          variant: 'outlined'
        }
      },
      MuiOutlinedInput: {
        styleOverrides: {
          root: {
            borderRadius: designTokens.radius.sm,
            backgroundColor: isDark ? alpha('#ffffff', 0.03) : '#ffffff',
            transition: 'box-shadow 140ms ease, background-color 140ms ease',
            '&:hover .MuiOutlinedInput-notchedOutline': {
              borderColor: isDark ? alpha('#ffffff', 0.22) : neutral[300]
            },
            '&.Mui-focused': {
              boxShadow: `0 0 0 4px ${alpha(isDark ? '#a78bfa' : '#4f46e5', 0.12)}`
            }
          },
          notchedOutline: {
            borderColor: isDark ? alpha('#ffffff', 0.12) : neutral[200]
          }
        }
      },
      MuiDialog: {
        styleOverrides: {
          paper: {
            borderRadius: designTokens.radius.xl,
            border: `1px solid ${isDark ? alpha('#ffffff', 0.1) : neutral[200]}`,
            boxShadow: designTokens.shadow.lg
          }
        }
      },
      MuiChip: {
        styleOverrides: {
          root: {
            borderRadius: 999,
            fontWeight: 700
          }
        }
      },
      MuiTooltip: {
        styleOverrides: {
          tooltip: {
            borderRadius: designTokens.radius.xs,
            fontSize: 12
          }
        }
      },
      MuiTableCell: {
        styleOverrides: {
          head: {
            fontSize: 12,
            fontWeight: 800,
            color: isDark ? neutral[300] : neutral[600],
            textTransform: 'uppercase',
            letterSpacing: '0.06em'
          }
        }
      }
    }
  });
}
