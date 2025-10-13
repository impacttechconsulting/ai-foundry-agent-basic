import React, { useState, useEffect, useRef } from 'react';
import './App.css';

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

const App: React.FC = () => {
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

  // Escape HTML to prevent XSS
  const escapeHTML = (str: string): string => {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
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
    <div className="chat-container">
      <header className="chat-header">
        <i className="fas fa-robot header-icon"></i>
        <h1>AI Foundry Agent</h1>
        <p>Intelligent assistance at your fingertips</p>
      </header>

      <main className="chat-messages" id="chatMessages">
        {isLoading && (
          <div className="welcome-message">
            <h2>Loading AI Assistant...</h2>
            <p>Please wait while we connect to the service.</p>
          </div>
        )}
        
        {!isLoading && messages.length === 0 && (
          <div className="welcome-message">
            <h2>Welcome to AI Foundry Agent!</h2>
            <p>I'm your intelligent assistant ready to help with various tasks and answer your questions.</p>
            <p>How can I assist you today?</p>
            
            <div className="suggestions">
              {suggestions.map((suggestion, index) => (
                <button 
                  key={index} 
                  className="suggestion-btn" 
                  onClick={() => handleSuggestion(suggestion)}
                >
                  {suggestion}
                </button>
              ))}
            </div>
          </div>
        )}

        {messages.map((message) => (
          <div key={message.id} className={`message ${message.type === 'user' ? 'user-message' : 'ai-message'}`}>
            <div className="message-content">
              <div className="message-info">
                <div className="message-sender">{message.sender}</div>
                <div className="message-time">{message.timestamp}</div>
              </div>
              <div className="message-text">{message.content}</div>
            </div>
          </div>
        ))}

        {isBotTyping && (
          <div className="message ai-message">
            <div className="message-content">
              <div className="typing-indicator">AI Assistant is typing...</div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </main>

      <div className="input-area">
        <input
          type="text"
          className="message-input"
          id="messageInput"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Type your message here..."
          autoComplete="off"
          aria-label="Type your message"
        />
        <button
          className="send-button"
          id="sendButton"
          type="button"
          onClick={sendMessage}
          aria-label="Send message"
          disabled={isLoading}
        >
          <i className="fas fa-paper-plane"></i>
        </button>
      </div>
    </div>
  );
};

export default App;
