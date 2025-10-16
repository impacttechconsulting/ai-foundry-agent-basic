import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Paper,
  TextField,
  Button,
  Avatar,
  Typography,
  Container,
  CircularProgress,
  InputAdornment,
  IconButton
} from '@mui/material';
import { Psychology as AiIcon } from '@mui/icons-material';
import ApiClient from '../utils/ApiClient';

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

const ChatAgent: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState<string>('');
  const [threadId, setThreadId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isBotTyping, setIsBotTyping] = useState<boolean>(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Format time to HH:MM format
  const formatTime = useCallback((date: Date): string => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }, []);

  // Add a new message to the chat
  const addMessage = useCallback((sender: string, type: 'user' | 'ai', content: string) => {
    const newMessage: Message = {
      id: Date.now().toString(),
      sender,
      content,
      timestamp: formatTime(new Date()),
      type,
    };
    setMessages(prev => [...prev, newMessage]);
  }, [formatTime]);

  // Create a new chat thread
  const createThread = useCallback(async (): Promise<ThreadResponse> => {
    const response = await ApiClient.post('/chat/threads');

    if (!response.ok) {
      const errorMessage = await response.text().catch(() => response.statusText);
      throw new Error(`Error creating session: ${errorMessage}`);
    }

    return response.json();
  }, []);

  // Send a prompt to the API
  const sendPrompt = useCallback(async (prompt: string): Promise<CompletionResponse> => {
    if (!threadId) {
      throw new Error('No active thread. Please refresh the page.');
    }

    const response = await ApiClient.post(`/chat/completions/${threadId}`, prompt);

    if (!response.ok) {
      const errorMessage = await response.text().catch(() => response.statusText);
      throw new Error(`Error sending prompt: ${errorMessage}`);
    }

    return response.json();
  }, [threadId]);

  // Handle sending a message
  const sendMessage = useCallback(async () => {
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
  }, [inputValue, threadId, addMessage, sendPrompt]);

  // Handle a suggestion
  const handleSuggestion = useCallback((text: string) => {
    setInputValue(text);
    setTimeout(() => {
      const sendButton = document.getElementById('sendButton');
      if (sendButton) sendButton.click();
    }, 100);
  }, []);

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
  }, [addMessage, createThread]);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isBotTyping]);

  // Handle Enter key press in input field
  const handleKeyPress = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      sendMessage();
    }
  }, [sendMessage]);

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
      height: '100%',
      maxHeight: '100vh',
      backgroundColor: 'background.default'
    }}>
      {/* Chat Header */}
      <Box sx={{ 
        backgroundColor: 'primary.main', 
        color: 'white', 
        padding: 2, 
        textAlign: 'center',
        flexShrink: 0
      }}>
        <Typography variant="h5" component="h1">
          <AiIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
          AI Foundry Agent
        </Typography>
        <Typography variant="body2">
          Intelligent assistance at your fingertips
        </Typography>
      </Box>

      {/* Main content area with messages and input */}
      <Box 
        sx={{ 
          flex: 1,
          display: 'flex',
          flexDirection: 'column'
        }}
      >
        {/* Chat Messages Area - This will scroll if needed */}
        <Box 
          sx={{ 
            flex: 1,
            padding: 2,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
            overflow: 'hidden' /* Prevent double scrollbars at this level */
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
          
          <Box 
            className={messages.length > 0 || isBotTyping ? 'chat-messages-container' : 'chat-messages-container no-scrollbar'}
            sx={{ 
              flex: 1,
              overflowY: messages.length > 0 || isBotTyping ? 'auto' : 'hidden',
              display: 'flex',
              flexDirection: 'column'
            }}
          >
        {!isLoading && messages.length === 0 && !isBotTyping && (
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
      </Box>
    </Box>

    {/* Input Area - Always stays at the bottom */}
    <Box sx={{ 
      padding: 2, 
      backgroundColor: 'background.paper',
      flexShrink: 0
    }}>
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

export default ChatAgent;