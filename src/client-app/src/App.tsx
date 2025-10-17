import React from 'react';
import {
  AppBar,
  Box,
  CssBaseline,
  Drawer,
  IconButton,
  Toolbar,
  Typography
} from '@mui/material';
import { Menu as MenuIcon } from '@mui/icons-material';
import './App.css';
import ApiClient from './utils/ApiClient';

// Import components from separate files
import Login from './components/Login';
import ChatAgent from './components/ChatAgent';
import Dashboard from './components/Dashboard';
import Rag from './components/Rag';
import ContentUnderstanding from './components/ContentUnderstanding';
import Navigation from './components/Navigation';

const drawerWidth = 240;

// Main App Component
const App: React.FC = () => {
  const [mobileOpen, setMobileOpen] = React.useState(false);
  const [currentPage, setCurrentPage] = React.useState('dashboard'); // Default to dashboard after login
  const [isLoggedIn, setIsLoggedIn] = React.useState(false);

  React.useEffect(() => {
    // Check if user is already logged in on initial load
    const token = localStorage.getItem('authToken');
    if (token) {
      // Optionally validate the token with the backend
      validateToken();
    } else {
      // If no token, show login page
      setCurrentPage('login');
      setIsLoggedIn(false);
    }
  }, []);

  const validateToken = async () => {
    try {
      const response = await ApiClient.get('/auth/validate');
      
      if (response.ok) {
        setIsLoggedIn(true);
        setCurrentPage('dashboard');
      } else {
        // Token is invalid, clear it and redirect to login
        localStorage.removeItem('authToken');
        localStorage.removeItem('username');
        setCurrentPage('login');
        setIsLoggedIn(false);
      }
    } catch (error) {
      console.error('Error validating token:', error);
      localStorage.removeItem('authToken');
      localStorage.removeItem('username');
      setCurrentPage('login');
      setIsLoggedIn(false);
    }
  };

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogin = () => {
    setIsLoggedIn(true);
    setCurrentPage('dashboard');
  };

  const handleLogout = () => {
    // Clear the authentication token
    localStorage.removeItem('authToken');
    localStorage.removeItem('username');
    
    setIsLoggedIn(false);
    setCurrentPage('login');
  };

  const handleNavigate = (page: string) => {
    setCurrentPage(page);
    setMobileOpen(false);
  };

  // Render login page if not logged in
  if (!isLoggedIn) {
    return <Login onLogin={handleLogin} />;
  }

  // Dashboard layout with navigation drawer
  const drawer = (
    <Navigation 
      currentPage={currentPage} 
      onNavigate={handleNavigate} 
      onLogout={handleLogout} 
    />
  );

  const container = window !== undefined ? () => window.document.body : undefined;

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div">
            {currentPage === 'dashboard' && 'Dashboard'}
            {currentPage === 'chat' && 'Chat Agent'}
            {currentPage === 'rag' && 'RAG'}
            {currentPage === 'content' && 'Content Understanding'}
          </Typography>
        </Toolbar>
      </AppBar>
      
      <Box
        component="nav"
        sx={{ 
          width: { sm: drawerWidth }, 
          flexShrink: { sm: 0 },
          height: '100vh',
          position: 'fixed',
          top: 0,
        }}
        aria-label="navigation folders"
      >
        <Drawer
          container={container}
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth, top: '64px' },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>
      
      <Box
        component="main"
        sx={{ flexGrow: 1, p: 3, width: { sm: `calc(100% - ${drawerWidth}px)` }, ml: { sm: `${drawerWidth}px` } }}
      >
        <Toolbar />
        
        {/* Render the appropriate page based on current selection */}
        {currentPage === 'dashboard' && <Dashboard />}
        {currentPage === 'chat' && <ChatAgent />}
        {currentPage === 'rag' && <Rag />}
        {currentPage === 'content' && <ContentUnderstanding />}
      </Box>
    </Box>
  );
};

export default App;