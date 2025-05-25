# UX/UI Style Guide for Dashboard Application

This style guide outlines the design principles and component specifications for the dashboard application, focusing on a bold, simple, and intuitive user experience within a dark theme.

## 1. Design Principles

Inspired by the provided aesthetics and user preferences, our design principles are:

*   **Bold Simplicity:** Clean, uncluttered interfaces with clear hierarchy.
*   **Intuitive Navigation:** Frictionless user journeys with easily discoverable elements.
*   **Breathable Whitespace:** Strategic use of negative space for cognitive breathing room and content prioritization.
*   **Strategic Color Accents:** Purposeful use of color to guide the eye and highlight key information, complementing the dark theme.
*   **Typography Hierarchy:** Clear visual distinction between different levels of information using font weight and size.
*   **Visual Density Optimization:** Balancing information availability with cognitive load management.
*   **Accessibility-Driven:** Ensuring high contrast ratios and universal usability.
*   **Feedback Responsiveness:** Clear communication of system status through subtle state transitions.
*   **Content-First Layouts:** Prioritizing user objectives over decorative elements.

## 2. Color Palette (Dark Theme)

The primary color scheme will be dark, with strategic use of lighter accents for readability and emphasis.

*   **Backgrounds:**
    *   `--color-dashboard-bg`: A deep, dark background color (e.g., `#1A1A2E` or similar to existing `bg-dashboard`).
    *   `--color-panel-bg`: A slightly lighter dark color for panels and cards (e.g., `#2C2C4A` or similar to existing `bg-panel`).
*   **Text Colors:**
    *   `--color-text-primary`: Off-white for primary text (e.g., `#E0E0E0` - similar to `text-gray-200`).
    *   `--color-text-secondary`: Lighter gray for secondary text/muted information (e.g., `#A0A0A0` - similar to `text-muted`).
    *   `--color-text-accent`: Color for highlighted text (e.g., a vibrant blue or green).
*   **Accent Colors:**
    *   `--color-accent-primary`: A vibrant blue for primary actions, links, and active states (e.g., `#007BFF` or similar to existing `bg-blue-600`).
    *   `--color-accent-secondary`: A complementary accent color (e.g., a subtle green or purple).
*   **Borders/Dividers:**
    *   `--color-border`: A subtle dark gray for borders and dividers (e.g., `#3A3A5A` or similar to existing `border-dashboard`).

## 3. Typography

*   **Font Family:** `Inter` (for headings and primary text) and `Noto Sans` (for secondary text/data, if needed for distinction). These are already in use.
*   **Headings (H1-H6):**
    *   `H1`: `text-2xl` (e.g., `font-bold`, `text-white`) - currently used for "Project Dashboard".
    *   `H2`: `text-lg` (e.g., `font-bold`, `text-white`) - currently used for "Data Insights".
    *   Further heading sizes will follow a proportional scale.
*   **Body Text:** `text-sm`, `font-medium` (e.g., `text-white`, `text-muted`).
*   **Data/Numbers:** `text-3xl` (e.g., `text-white`) for large metrics.

## 4. Spacing & Layout

*   **Grid System:** Implicitly using Tailwind's default spacing scale (e.g., `gap-4`, `p-6`, `px-10`, `py-3`). A base unit of `4px` or `8px` is recommended for consistency.
*   **Whitespace:** Generous use of padding and margins around components and sections to ensure "cognitive breathing room."
*   **Component Spacing:** Consistent `gap` values for elements within components (e.g., `gap-3` for sidebar items).

## 5. Iconography

*   **Library:** Feather Icons.
*   **Color:** `text-gray-200` (off-white) for default state.
*   **Usage:** Paired with text in navigation and key information points.

## 6. Components (Conceptual)

### Buttons
*   **Primary:** `rounded bg-blue-600 px-4 py-2 text-white font-bold` (e.g., "Submit" button).
*   **Secondary/Ghost:** Styles to be defined, potentially `bg-panel` with `text-white` and a border.
*   **States:** Hover (subtle background change), Active (slight press effect), Disabled (reduced opacity).

### Input Fields
*   **Default:** `rounded border p-2 text-black` (current style).
*   **Focus:** Border color change (e.g., to `--color-accent-primary`).
*   **Placeholder:** `text-muted`.

### Navigation
*   **Top Menu:** `text-white text-sm font-medium flex items-center gap-1` with `hover:text-gray-400` (or similar subtle change).
*   **Sidebar:** `flex items-center gap-3 p-2 hover:bg-panel rounded` for `<a>` tags. Active state should have a distinct background (e.g., `bg-panel` as currently used for "Charts").

### Cards/Containers
*   **Default:** `rounded border border-dashboard p-6 bg-dashboard`.
*   **Content:** `text-white` for titles/main data, `text-muted` for descriptions.

### Charts
*   **Placeholder:** `rounded border border-dashboard p-6 bg-dashboard h-64 flex items-center justify-center`.
*   **Future:** Colors should align with the accent palette.

## 7. MCP Server Integration

### Context7
*   Can be used to resolve and fetch documentation for specific libraries or design patterns that might be considered for future enhancements (e.g., charting libraries, advanced UI components).
*   Example usage: `use_mcp_tool` with `resolve-library-id` and `get-library-docs` for `chart.js` or `recharts`.

### Shadcn MCP
*   The `create_note` tool can be used to document specific design decisions, component variations, or feedback received during the design process.
*   Example usage: `use_mcp_tool` with `create_note` to log a decision about a specific button style.

## 8. Future Considerations

*   **Motion Choreography:** Implement physics-based transitions for spatial continuity (e.g., for sidebar expansion/collapse, modal animations).
*   **Accessibility:** Conduct thorough accessibility audits (WCAG compliance) for color contrast, keyboard navigation, and screen reader compatibility.
*   **Component States:** Define all possible states for interactive components (e.g., error, success, loading).

This style guide provides a foundational framework. As the project evolves, it can be expanded with more detailed component specifications, interaction patterns, and a living style guide implementation.
