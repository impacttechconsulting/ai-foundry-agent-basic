import React, { useState, useEffect, useRef, lazy, Suspense } from 'react';
import {
  AppBar,
  Box,
  CssBaseline,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Container,
  CircularProgress,
  TextField,
  Button,
  Avatar,
  Paper,
  InputAdornment
} from '@mui/material';
import {
  Menu as MenuIcon,
  Chat as ChatIcon,
  Login as LoginIcon,
  Logout as LogoutIcon,
  Dashboard as DashboardIcon,
  AutoFixHigh as AgentIcon,
  Psychology as AiIcon,
  Description as DocumentIcon,
  Search as SearchIcon
} from '@mui/icons-material';
import './App.css';

// Define interfaces
interface Message {
  id: string;
  sender: string;
  content: string;
  timestamp: string;
  type: 'user' | 'ai';
}

interface ThreadResponse {
  id: string;
}

interface CompletionResponse {
  data: string;
}

const drawerWidth = 240;

// Chat Component - Separate component for the chat interface
const ChatInterface: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState<string>('');
  const [threadId, setThreadId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isBotTyping, setIsBotTyping] = useState<boolean>(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Format time to HH:MM format
  const formatTime = (date: Date): string => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  // Add a new message to the chat
  const addMessage = (sender: string, type: 'user' | 'ai', content: string) => {
    const newMessage: Message = {
      id: Date.now().toString(),
      sender,
      content,
      timestamp: formatTime(new Date()),
      type,
    };
    setMessages(prev => [...prev, newMessage]);
  };

  // Create a new chat thread
  const createThread = async (): Promise<ThreadResponse> => {
    const response = await fetch('/chat/threads', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const errorMessage = await response.text().catch(() => response.statusText);
      throw new Error(`Error creating session: ${errorMessage}`);
    }

    return response.json();
  };

  // Send a prompt to the API
  const sendPrompt = async (prompt: string): Promise<CompletionResponse> => {
    if (!threadId) {
      throw new Error('No active thread. Please refresh the page.');
    }

    const response = await fetch(`/chat/completions/${threadId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(prompt),
    });

    if (!response.ok) {
      const errorMessage = await response.text().catch(() => response.statusText);
      throw new Error(`Error sending prompt: ${errorMessage}`);
    }

    return response.json();
  };

  // Handle sending a message
  const sendMessage = async () => {
    if (!inputValue.trim() || !threadId) return;

    const prompt = inputValue.trim();
    setInputValue('');
    
    // Add user message to UI
    addMessage('You', 'user', prompt);

    // Show typing indicator
    setIsBotTyping(true);

    try {
      // Send prompt to backend
      const response = await sendPrompt(prompt);
      
      // Add AI response to UI
      addMessage('AI Assistant', 'ai', response.data);
    } catch (error) {
      console.error('Error sending message:', error);
      addMessage('AI Assistant', 'ai', 'Sorry, something went wrong. Please try again.');
    } finally {
      setIsBotTyping(false);
    }
  };

  // Handle a suggestion
  const handleSuggestion = (text: string) => {
    setInputValue(text);
    setTimeout(() => {
      const sendButton = document.getElementById('sendButton');
      if (sendButton) sendButton.click();
    }, 100);
  };

  // Initialize the chat on component mount
  useEffect(() => {
    const initializeChat = async () => {
      try {
        const { id } = await createThread();
        setThreadId(id);
        addMessage('AI Assistant', 'ai', 'Hello! I\'m your AI assistant. How can I help you today?');
      } catch (error) {
        console.error('Error initializing chat:', error);
        addMessage('AI Assistant', 'ai', 'Sorry, I\'m having trouble connecting. Please try again later.');
      } finally {
        setIsLoading(false);
      }
    };

    initializeChat();
  }, []);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isBotTyping]);

  // Handle Enter key press in input field
  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      sendMessage();
    }
  };

  // Suggested prompts
  const suggestions = [
    'Explain machine learning',
    'What are the benefits of AI?',
    'How to get started with Azure?',
    'Tell me about .NET 10'
  ];

  return (
    <Box sx={{ 
      display: 'flex', 
      flexDirection: 'column', 
      height: '100vh', 
      backgroundColor: 'background.default' 
    }}>
      {/* Chat Header */}
      <Box sx={{ 
        backgroundColor: 'primary.main', 
        color: 'white', 
        padding: 2, 
        textAlign: 'center' 
      }}>
        <Typography variant="h5" component="h1">
          <AiIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
          AI Foundry Agent
        </Typography>
        <Typography variant="body2">
          Intelligent assistance at your fingertips
        </Typography>
      </Box>

      {/* Chat Messages Area */}
      <Box 
        sx={{ 
          flex: 1, 
          padding: 2, 
          overflowY: 'auto',
          display: 'flex',
          flexDirection: 'column',
          gap: 2
        }}
      >
        {isLoading && (
          <Box 
            sx={{ 
              display: 'flex', 
              justifyContent: 'center', 
              alignItems: 'center', 
              height: '100%',
              flexDirection: 'column'
            }}
          >
            <CircularProgress color="primary" />
            <Typography variant="h6" sx={{ mt: 2 }}>
              Loading AI Assistant...
            </Typography>
            <Typography variant="body2">
              Please wait while we connect to the service.
            </Typography>
          </Box>
        )}
        
        {!isLoading && messages.length === 0 && (
          <Box 
            sx={{ 
              display: 'flex', 
              flexDirection: 'column', 
              alignItems: 'center', 
              justifyContent: 'center', 
              height: '100%', 
              textAlign: 'center',
              padding: 4
            }}
          >
            <Avatar sx={{ 
              width: 80, 
              height: 80, 
              mb: 2, 
              bgcolor: 'primary.main' 
            }}>
              <AiIcon sx={{ fontSize: 40 }} />
            </Avatar>
            <Typography variant="h4" component="h2" gutterBottom>
              Welcome to AI Foundry Agent!
            </Typography>
            <Typography variant="body1" sx={{ mb: 3 }}>
              I'm your intelligent assistant ready to help with various tasks and answer your questions.
            </Typography>
            <Typography variant="body1" sx={{ mb: 3 }}>
              How can I assist you today?
            </Typography>
            
            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center', gap: 1, mt: 2 }}>
              {suggestions.map((suggestion, index) => (
                <Button
                  key={index}
                  variant="outlined"
                  onClick={() => handleSuggestion(suggestion)}
                  sx={{ 
                    textTransform: 'none',
                    borderColor: 'primary.main',
                    color: 'primary.main',
                    '&:hover': {
                      backgroundColor: 'primary.main',
                      color: 'white'
                    }
                  }}
                >
                  {suggestion}
                </Button>
              ))}
            </Box>
          </Box>
        )}

        {messages.map((message) => (
          <Box
            key={message.id}
            sx={{
              display: 'flex',
              justifyContent: message.type === 'user' ? 'flex-end' : 'flex-start',
              animation: 'fadeIn 0.3s ease-out'
            }}
          >
            <Paper
              sx={{
                padding: 2,
                maxWidth: '80%',
                backgroundColor: message.type === 'user' ? 'primary.main' : 'grey.100',
                color: message.type === 'user' ? 'white' : 'text.primary',
                borderRadius: message.type === 'user' ? '20px 20px 4px 20px' : '20px 20px 20px 4px',
                boxShadow: 2
              }}
            >
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }}>
                  {message.sender}
                </Typography>
                <Typography variant="caption" sx={{ opacity: 0.7 }}>
                  {message.timestamp}
                </Typography>
              </Box>
              <Typography variant="body1">
                {message.content}
              </Typography>
            </Paper>
          </Box>
        ))}

        {isBotTyping && (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'flex-start',
            }}
          >
            <Paper
              sx={{
                padding: 2,
                maxWidth: '80%',
                backgroundColor: 'grey.100',
                color: 'text.primary',
                borderRadius: '20px 20px 20px 4px',
                boxShadow: 2
              }}
            >
              <Typography variant="body2" sx={{ fontStyle: 'italic' }}>
                AI Assistant is typing...
              </Typography>
            </Paper>
          </Box>
        )}

        <div ref={messagesEndRef} />
      </Box>

      {/* Input Area */}
      <Box sx={{ padding: 2, backgroundColor: 'background.paper' }}>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            fullWidth
            variant="outlined"
            placeholder="Type your message here..."
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={sendMessage}
                    disabled={isLoading || !inputValue.trim()}
                    edge="end"
                  >
                    <i className="fas fa-paper-plane"></i>
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
          <Button
            variant="contained"
            color="primary"
            onClick={sendMessage}
            disabled={isLoading || !inputValue.trim()}
            sx={{ minWidth: 55, height: 55 }}
            id="sendButton"
          >
            <i className="fas fa-paper-plane"></i>
          </Button>
        </Box>
      </Box>
    </Box>
  );
};

// Login Component
const Login: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Implement actual login logic
    console.log('Login attempt with:', { username, password });
    alert(`Login attempted with username: ${username}`);
  };

  return (
    <Container maxWidth="xs" sx={{ 
      display: 'flex', 
      flexDirection: 'column', 
      justifyContent: 'center', 
      alignItems: 'center', 
      minHeight: '100vh' 
    }}>
      <Paper elevation={3} sx={{ padding: 4, width: '100%' }}>
        <Box sx={{ textAlign: 'center', mb: 3 }}>
          <Avatar sx={{ 
            width: 64, 
            height: 64, 
            mx: 'auto', 
            bgcolor: 'primary.main' 
          }}>
            <LoginIcon />
          </Avatar>
          <Typography component="h1" variant="h5" sx={{ mt: 2 }}>
            Sign in to AI Foundry
          </Typography>
        </Box>

        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 1 }}>
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
            sx={{ mt: 3, mb: 2, py: 1.5 }}
          >
            Sign In
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

// Main App Component with Navigation
const App: React.FC = () => {
  const [mobileOpen, setMobileOpen] = React.useState(false);
  const [currentPage, setCurrentPage] = React.useState('chat'); // Default to chat
  const [isLoggedIn, setIsLoggedIn] = React.useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogin = () => {
    // For demo purposes, just toggle login state
    setIsLoggedIn(true);
    setCurrentPage('chat');
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setCurrentPage('login');
  };

  const drawer = (
    <div>
      <Toolbar sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center',
        backgroundColor: 'primary.main',
        color: 'white'
      }}>
        <Typography variant="h6" noWrap component="div">
          AI Foundry
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {!isLoggedIn && (
          <ListItem key="login" disablePadding>
            <ListItemButton 
              onClick={() => { 
                setCurrentPage('login'); 
                setMobileOpen(false); 
              }}
              selected={currentPage === 'login'}
            >
              <ListItemIcon>
                <LoginIcon />
              </ListItemIcon>
              <ListItemText primary="Login" />
            </ListItemButton>
          </ListItem>
        )}
        
        {isLoggedIn && (
          <>
            <ListItem key="dashboard" disablePadding>
              <ListItemButton 
                onClick={() => { 
                  setCurrentPage('dashboard'); 
                  setMobileOpen(false); 
                }}
                selected={currentPage === 'dashboard'}
              >
                <ListItemIcon>
                  <DashboardIcon />
                </ListItemIcon>
                <ListItemText primary="Dashboard" />
              </ListItemButton>
            </ListItem>
            
            <ListItem key="agent" disablePadding>
              <ListItemButton 
                onClick={() => { 
                  setCurrentPage('chat'); 
                  setMobileOpen(false); 
                }}
                selected={currentPage === 'chat'}
              >
                <ListItemIcon>
                  <AgentIcon />
                </ListItemIcon>
                <ListItemText primary="AI Agent" />
              </ListItemButton>
            </ListItem>
            
            <ListItem key="rag" disablePadding>
              <ListItemButton 
                onClick={() => { 
                  setCurrentPage('rag'); 
                  setMobileOpen(false); 
                }}
                selected={currentPage === 'rag'}
              >
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText primary="RAG Demo" />
              </ListItemButton>
            </ListItem>
            
            <ListItem key="content" disablePadding>
              <ListItemButton 
                onClick={() => { 
                  setCurrentPage('content'); 
                  setMobileOpen(false); 
                }}
                selected={currentPage === 'content'}
              >
                <ListItemIcon>
                  <DocumentIcon />
                </ListItemIcon>
                <ListItemText primary="Content Understanding" />
              </ListItemButton>
            </ListItem>
            
            <Divider sx={{ my: 1 }} />
            
            <ListItem key="logout" disablePadding>
              <ListItemButton onClick={handleLogout}>
                <ListItemIcon>
                  <LogoutIcon />
                </ListItemIcon>
                <ListItemText primary="Logout" />
              </ListItemButton>
            </ListItem>
          </>
        )}
      </List>
    </div>
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
            {currentPage === 'chat' && 'AI Agent Chat'}
            {currentPage === 'login' && 'Login'}
            {currentPage === 'dashboard' && 'Dashboard'}
            {currentPage === 'rag' && 'RAG Demo'}
            {currentPage === 'content' && 'Content Understanding'}
          </Typography>
        </Toolbar>
      </AppBar>
      
      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
        aria-label="mailbox folders"
      >
        <Drawer
          container={container}
          variant="temporary"
          open={mobileOpen}
          onTransitionEnd={() => setMobileOpen(false)} // Close when transition ends
          onClose={() => {}}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
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
        sx={{ flexGrow: 1, p: 3, width: { sm: `calc(100% - ${drawerWidth}px)` } }}
      >
        <Toolbar />
        
        {/* Render the appropriate page based on current selection */}
        {currentPage === 'login' && <Login />}
        {currentPage === 'chat' && <ChatInterface />}
        {currentPage === 'dashboard' && (
          <Container maxWidth="lg">
            <Typography variant="h4" gutterBottom>
              Dashboard
            </Typography>
            <Typography variant="body1">
              Welcome to the AI Foundry Dashboard. This is where you can manage your AI services.
            </Typography>
          </Container>
        )}
        {currentPage === 'rag' && (
          <Container maxWidth="lg">
            <Typography variant="h4" gutterBottom>
              RAG Demo
            </Typography>
            <Typography variant="body1">
              This is where the Retrieval Augmented Generation demo will be implemented.
            </Typography>
          </Container>
        )}
        {currentPage === 'content' && (
          <Container maxWidth="lg">
            <Typography variant="h4" gutterBottom>
              Content Understanding
            </Typography>
            <Typography variant="body1">
              This is where the content understanding feature will be implemented.
            </Typography>
          </Container>
        )}
      </Box>
    </Box>
  );
};

export default App;
