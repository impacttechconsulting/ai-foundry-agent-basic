import React, { useState } from 'react';
import {
  Container,
  Paper,
  Box,
  TextField,
  Button,
  Avatar,
  Typography,
  Alert,
  CircularProgress
} from '@mui/material';
import { Login as LoginIcon } from '@mui/icons-material';
import ApiClient from '../utils/ApiClient';

interface LoginProps {
  onLogin: () => void;
}

const Login: React.FC<LoginProps> = ({ onLogin }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    
    try {
      // Call the backend authentication API using ApiClient
      const response = await ApiClient.post('/auth/login', { username, password });

      const data = await response.json();

      if (response.ok && data.success) {
        // Store the token in localStorage (or sessionStorage)
        localStorage.setItem('authToken', data.token);
        localStorage.setItem('username', data.username);
        
        // Set authorization header for future requests
        // This is important for the React app to send the token with API requests
        onLogin(); // This will trigger navigation to the dashboard
      } else {
        setError(data.message || 'Login failed');
      }
    } catch (err) {
      setError('An error occurred while logging in');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ 
      display: 'flex', 
      flexDirection: { xs: 'column', md: 'row' }, 
      justifyContent: 'center', 
      alignItems: 'center', 
      minHeight: '100vh',
      padding: { xs: 3, sm: 4, md: 5 }
    }}>
      {/* Top side - Image for mobile, Left side for desktop */}
      <Box sx={{ 
        display: 'flex',
        justifyContent: 'center', 
        alignItems: 'center', 
        mb: { xs: 3, sm: 4 },
        mt: { xs: 2, sm: 3 }
      }}>
        <Box
          component="img"
          src="/TechX.jpg"
          alt="TechX Logo"
          sx={{
            maxWidth: { xs: '80%', sm: '60%', md: '100%' },
            maxHeight: { xs: '200px', md: '40vh' },
            width: 'auto',
            objectFit: 'contain',
            borderRadius: 2,
            boxShadow: 3
          }}
        />
      </Box>

      {/* Login Form */}
      <Paper elevation={3} sx={{ 
        padding: { xs: 3, sm: 4 }, 
        width: { xs: '100%', sm: '380px', md: '400px' },
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        mt: { xs: 1, md: 0 },
        boxShadow: 4
      }}>
        <Box sx={{ textAlign: 'center', mb: 3 }}>
          <Avatar sx={{ 
            width: 64, 
            height: 64, 
            mx: 'auto', 
            bgcolor: 'primary.main' 
          }}>
            <LoginIcon />
          </Avatar>
          <Typography component="h1" variant="h5" sx={{ mt: 2, fontWeight: 'bold' }}>
            Sign in to AI Foundry
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="username"
            label="Username"
            name="username"
            autoComplete="username"
            autoFocus
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            name="password"
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2, py: 1.5, fontWeight: 'bold' }}
            disabled={loading}
          >
            {loading ? (
              <>
                <CircularProgress size={20} sx={{ mr: 1 }} />
                Signing In...
              </>
            ) : (
              'Sign In'
            )}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Login;