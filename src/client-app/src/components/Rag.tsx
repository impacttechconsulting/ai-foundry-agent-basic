import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Paper,
  TextField,
  Button,
  Avatar,
  Typography,
  CircularProgress,
  InputAdornment,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Collapse,
  Alert,
  LinearProgress,
  Chip
} from '@mui/material';
import { 
  Psychology as AiIcon, 
  AttachFile as AttachFileIcon, 
  ExpandMore as ExpandMoreIcon, 
  ExpandLess as ExpandLessIcon,
  Description as DocumentIcon
} from '@mui/icons-material';
import ApiClient from '../utils/ApiClient';

// Define interfaces
interface SourceDocument {
  Title: string;
  Content: string;
  Url: string;
}

interface Message {
  id: string;
  sender: string;
  content: string;
  timestamp: string;
  type: 'user' | 'ai';
  sources?: SourceDocument[];
}

interface CompletionResponse {
  data: string;
  sources?: SourceDocument[];
}

interface ThreadResponse {
  id: string;
}

const RagDemo: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState<string>('');
  const [threadId, setThreadId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isBotTyping, setIsBotTyping] = useState<boolean>(false);
  const [file, setFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  const [isUploading, setIsUploading] = useState<boolean>(false);
  const [uploadMessage, setUploadMessage] = useState<string>('');
  const [showSources, setShowSources] = useState<boolean>(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Format time to HH:MM format
  const formatTime = useCallback((date: Date): string => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }, []);

  // Add a new message to the chat
  const addMessage = useCallback((sender: string, type: 'user' | 'ai', content: string, sources?: SourceDocument[]) => {
    const newMessage: Message = {
      id: Date.now().toString(),
      sender,
      content,
      timestamp: formatTime(new Date()),
      type,
      sources
    };
    setMessages(prev => [...prev, newMessage]);
  }, [formatTime]);

  // Create a new RAG thread
  const createThread = useCallback(async (): Promise<ThreadResponse> => {
    const response = await ApiClient.post('/api/rag/threads');

    if (!response.ok) {
      const errorMessage = await response.text().catch(() => response.statusText);
      throw new Error(`Error creating RAG session: ${errorMessage}`);
    }

    return response.json();
  }, []);

  // Send a prompt to the RAG API
  const sendPrompt = useCallback(async (prompt: string): Promise<CompletionResponse> => {
    if (!threadId) {
      throw new Error('No active thread. Please refresh the page.');
    }

    const response = await ApiClient.post(`/api/rag/completions/${threadId}`, {
      prompt,
      documentIds: []
    });

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
      
      // Add AI response to UI with sources if available
      addMessage('RAG Assistant', 'ai', response.data, response.sources);
    } catch (error) {
      console.error('Error sending message:', error);
      addMessage('RAG Assistant', 'ai', 'Sorry, something went wrong. Please try again.');
    } finally {
      setIsBotTyping(false);
    }
  }, [inputValue, threadId, addMessage, sendPrompt]);

  // Handle file selection
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0];
      const allowedTypes = ['application/pdf', 'text/plain', 'application/msword', 
                           'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
                           'application/vnd.ms-excel', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'];
      
      if (!allowedTypes.includes(selectedFile.type)) {
        setUploadMessage('File type not supported. Please upload PDF, TXT, DOC, DOCX, XLS, or XLSX files.');
        setFile(null);
        return;
      }
      
      if (selectedFile.size > 10 * 1024 * 1024) { // 10MB limit
        setUploadMessage('File size exceeds 10MB limit.');
        setFile(null);
        return;
      }
      
      setFile(selectedFile);
      setUploadMessage(`Selected: ${selectedFile.name}`);
    }
  };

  // Handle file upload
  const handleFileUpload = async () => {
    if (!file || !threadId) return;

    setIsUploading(true);
    setUploadMessage('Uploading file...');
    
    try {
      const formData = new FormData();
      formData.append('file', file);

      const xhr = new XMLHttpRequest();
      
      // Handle upload progress
      xhr.upload.onprogress = (event) => {
        if (event.lengthComputable) {
          const progress = Math.round((event.loaded / event.total) * 100);
          setUploadProgress(progress);
        }
      };

      xhr.open('POST', '/api/rag/upload');
      
      // Set authorization header
      const token = localStorage.getItem('authToken');
      if (token) {
        xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      }

      xhr.onload = () => {
        setIsUploading(false);
        setUploadProgress(0);
        
        if (xhr.status === 200) {
          const response = JSON.parse(xhr.responseText);
          setUploadMessage(response.message);
          setFile(null);
          // Clear input to allow re-upload of same file
          const fileInput = document.getElementById('file-upload') as HTMLInputElement;
          if (fileInput) fileInput.value = '';
          
          addMessage('System', 'ai', `File "${response.fileName}" uploaded and indexed successfully.`);
        } else {
          const errorResponse = JSON.parse(xhr.responseText);
          setUploadMessage(`Upload failed: ${errorResponse.error || 'Unknown error'}`);
        }
      };

      xhr.onerror = () => {
        setIsUploading(false);
        setUploadProgress(0);
        setUploadMessage('Upload failed due to network error.');
      };

      xhr.send(formData);
    } catch (error) {
      console.error('Error uploading file:', error);
      setIsUploading(false);
      setUploadProgress(0);
      setUploadMessage('Upload failed due to an error.');
    }
  };

  // Handle a suggestion
  const handleSuggestion = useCallback((text: string) => {
    setInputValue(text);
    setTimeout(() => {
      const sendButton = document.getElementById('sendButton');
      if (sendButton) sendButton.click();
    }, 100);
  }, []);

  // Initialize the RAG chat on component mount
  useEffect(() => {
    const initializeRag = async () => {
      try {
        const { id } = await createThread();
        setThreadId(id);
        addMessage('RAG Assistant', 'ai', 'Hello! I\'m your RAG assistant. I can answer questions based on uploaded documents. How can I help you today?');
      } catch (error) {
        console.error('Error initializing RAG:', error);
        addMessage('RAG Assistant', 'ai', 'Sorry, I\'m having trouble connecting. Please try again later.');
      } finally {
        setIsLoading(false);
      }
    };

    initializeRag();
  }, [addMessage, createThread]);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isBotTyping]);

  // Handle Enter key press in input field
  const handleKeyPress = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  }, [sendMessage]);

  // Suggested prompts
  const suggestions = [
    'Explain the latest company policies',
    'What are the key points from the quarterly report?',
    'How do I submit my expense report?',
    'What is the procedure for requesting time off?'
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
          RAG Assistant
        </Typography>
        <Typography variant="body2">
          Retrieval-Augmented Generation powered by Azure AI Search
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
                Loading RAG Assistant...
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
                  Welcome to RAG Assistant!
                </Typography>
                <Typography variant="body1" sx={{ mb: 3 }}>
                  I'm your intelligent assistant powered by Retrieval-Augmented Generation.
                  I can answer questions based on your documents and knowledge base.
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
                  
                  {/* Show sources if this is an AI response with sources */}
                  {message.type === 'ai' && message.sender === 'RAG Assistant' && message.sources && message.sources.length > 0 && (
                    <Box sx={{ mt: 1, pt: 1, borderTop: '1px solid #e0e0e0' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                        <Typography variant="caption" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                          Sources:
                        </Typography>
                        <IconButton 
                          size="small" 
                          onClick={() => setShowSources(!showSources)}
                          sx={{ padding: 0 }}
                        >
                          {showSources ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
                        </IconButton>
                      </Box>
                      
                      <Collapse in={showSources}>
                        <List dense sx={{ mt: 1, pl: 2 }}>
                          {message.sources.map((source, index) => (
                            <ListItem key={index} sx={{ pl: 0, py: 0.5 }}>
                              <ListItemIcon sx={{ minWidth: '24px', color: 'primary.main' }}>
                                <DocumentIcon fontSize="small" />
                              </ListItemIcon>
                              <ListItemText 
                                primary={source.Title} 
                                secondary={source.Content.substring(0, 100) + (source.Content.length > 100 ? '...' : '')}
                                primaryTypographyProps={{ variant: 'caption', fontWeight: 'medium' }}
                                secondaryTypographyProps={{ variant: 'caption' }}
                              />
                            </ListItem>
                          ))}
                        </List>
                      </Collapse>
                    </Box>
                  )}
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
                    RAG Assistant is thinking...
                  </Typography>
                </Paper>
              </Box>
            )}

            <div ref={messagesEndRef} />
          </Box>
        </Box>
      </Box>

      {/* File Upload Area */}
      <Box sx={{ 
        padding: 2, 
        backgroundColor: 'background.paper',
        borderTop: '1px solid #e0e0e0',
        flexShrink: 0
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <input
            accept=".pdf,.txt,.doc,.docx,.xls,.xlsx"
            style={{ display: 'none' }}
            id="file-upload"
            type="file"
            onChange={handleFileChange}
          />
          <label htmlFor="file-upload">
            <Button 
              variant="outlined" 
              component="span"
              startIcon={<AttachFileIcon />}
              disabled={isUploading}
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
              Upload Document
            </Button>
          </label>
          
          {file && (
            <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
              <Chip 
                label={file.name} 
                size="small" 
                onDelete={() => {
                  setFile(null);
                  setUploadMessage('');
                  const fileInput = document.getElementById('file-upload') as HTMLInputElement;
                  if (fileInput) fileInput.value = '';
                }}
              />
              <Button 
                variant="contained" 
                color="primary" 
                onClick={handleFileUpload}
                disabled={isUploading}
                size="small"
              >
                {isUploading ? 'Uploading...' : 'Upload'}
              </Button>
            </Box>
          )}
        </Box>
        
        {uploadMessage && (
          <Alert 
            severity={uploadMessage.includes('failed') || uploadMessage.includes('error') ? 'error' : 'info'}
            sx={{ mt: 1 }}
          >
            {uploadMessage}
          </Alert>
        )}
        
        {isUploading && (
          <Box sx={{ width: '100%', mt: 1 }}>
            <LinearProgress variant="determinate" value={uploadProgress} />
          </Box>
        )}
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
            placeholder="Ask a question about your documents..."
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            multiline
            maxRows={4}
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

export default RagDemo;