import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Snackbar,
  Alert,
  Card,
  CardContent,
  Divider,
  Chip,
  CircularProgress
} from '@mui/material';
import { Save as SaveIcon, Add as AddIcon, Update as UpdateIcon } from '@mui/icons-material';
import ApiClient from '../utils/ApiClient';

interface ModelDeployment {
  id: string;
  name: string;
  model: string;
  version: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

interface Agent {
  id: string;
  name: string;
  description: string;
  model: string;
  instructions: string;
}

const AgentCreation: React.FC = () => {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [modelDeployments, setModelDeployments] = useState<ModelDeployment[]>([]);
  const [selectedAgentId, setSelectedAgentId] = useState<string>('');
  const [formData, setFormData] = useState({
    id: '',
    name: '',
    description: '',
    model: '',
    instructions: ''
  });
  const [loading, setLoading] = useState(false);
  const [loadingAgents, setLoadingAgents] = useState(true);
  const [loadingModels, setLoadingModels] = useState(true);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  // Fetch agents and model deployments from API
  useEffect(() => {
    fetchAgents();
    fetchModelDeployments();
  }, []);

  // Update form when agent is selected
  useEffect(() => {
    if (selectedAgentId) {
      const agent = agents.find(a => a.id === selectedAgentId);
      if (agent) {
        setFormData({
          id: agent.id,
          name: agent.name,
          description: agent.description,
          model: agent.model,
          instructions: agent.instructions
        });
      }
    } else {
      resetForm();
    }
  }, [selectedAgentId, agents]);

  const fetchAgents = async () => {
    try {
      setLoadingAgents(true);
      const response = await ApiClient.get('/api/agents');
      if (response.ok) {
        const data = await response.json();
        setAgents(data.agents || []);
      } else {
        throw new Error('Failed to fetch agents');
      }
    } catch (error) {
      console.error('Error fetching agents:', error);
      showSnackbar('Failed to fetch agents', 'error');
    } finally {
      setLoadingAgents(false);
    }
  };

  const fetchModelDeployments = async () => {
    try {
      setLoadingModels(true);
      const response = await ApiClient.get('/api/agents/models');
      if (response.ok) {
        const data = await response.json();
        setModelDeployments(data || []);
      } else {
        throw new Error('Failed to fetch model deployments');
      }
    } catch (error) {
      console.error('Error fetching model deployments:', error);
      showSnackbar('Failed to fetch model deployments', 'error');
    } finally {
      setLoadingModels(false);
    }
  };

  const resetForm = () => {
    setFormData({
      id: '',
      name: '',
      description: '',
      model: '',
      instructions: ''
    });
    setSelectedAgentId('');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleModelChange = (e: { target: { value: string } }) => {
    const { value } = e.target;
    setFormData(prev => ({
      ...prev,
      model: value
    }));
  };

  const handleCreateAgent = async () => {
    if (!formData.name || !formData.model || !formData.instructions) {
      showSnackbar('Name, Model, and Instructions are required', 'error');
      return;
    }

    setLoading(true);
    try {
      const response = await ApiClient.post('/api/agents', {
        name: formData.name,
        description: formData.description,
        model: formData.model,
        instructions: formData.instructions
      });

      if (response.ok) {
        const newAgent = await response.json();
        setAgents(prev => [...prev, newAgent]);
        resetForm();
        showSnackbar('Agent created successfully', 'success');
        await fetchAgents(); // Refresh the list
      } else {
        const errorData = await response.json();
        showSnackbar(errorData.error || 'Failed to create agent', 'error');
      }
    } catch (error) {
      console.error('Error creating agent:', error);
      showSnackbar('Error creating agent', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateAgent = async () => {
    if (!formData.id || !formData.name || !formData.model || !formData.instructions) {
      showSnackbar('Name, Model, and Instructions are required', 'error');
      return;
    }

    setLoading(true);
    try {
      const response = await ApiClient.put(`/api/agents/${formData.id}`, {
        name: formData.name,
        description: formData.description,
        model: formData.model,
        instructions: formData.instructions
      });

      if (response.ok) {
        const updatedAgent = await response.json();
        setAgents(prev => prev.map(a => a.id === updatedAgent.id ? updatedAgent : a));
        showSnackbar('Agent updated successfully', 'success');
        await fetchAgents(); // Refresh the list
      } else {
        const errorData = await response.json();
        showSnackbar(errorData.error || 'Failed to update agent', 'error');
      }
    } catch (error) {
      console.error('Error updating agent:', error);
      showSnackbar('Error updating agent', 'error');
    } finally {
      setLoading(false);
    }
  };

  const showSnackbar = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleCloseSnackbar = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  const isNewAgent = !formData.id;

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5', padding: 3 }}>
      <Container maxWidth="md">
        <Paper 
          elevation={3} 
          sx={{ 
            p: 4, 
            backgroundColor: 'white',
            borderRadius: 4,
            boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
            overflowY: 'auto',
          }}
        >
          <Typography 
            variant="h4" 
            component="h1" 
            gutterBottom 
            color="primary" 
            sx={{ fontWeight: 'bold', textAlign: 'center' }}
          >
            Agent Creation & Management
          </Typography>
          
          <Divider sx={{ my: 2 }} />
          
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Box>
              <FormControl fullWidth variant="outlined" margin="normal">
                <InputLabel id="agent-select-label">Select Agent</InputLabel>
                <Select
                  labelId="agent-select-label"
                  id="agent-select"
                  value={selectedAgentId}
                  onChange={(e) => setSelectedAgentId(e.target.value as string)}
                  label="Select Agent"
                  disabled={loadingAgents}
                >
                  <MenuItem value="">
                    <em>New Agent</em>
                  </MenuItem>
                  {loadingAgents ? (
                    <MenuItem disabled>
                      <CircularProgress size={20} sx={{ mr: 1 }} />
                      Loading agents...
                    </MenuItem>
                  ) : agents.length > 0 ? (
                    agents.map(agent => (
                      <MenuItem key={agent.id} value={agent.id}>
                        {agent.name} ({agent.model})
                      </MenuItem>
                    ))
                  ) : (
                    <MenuItem disabled>No agents available</MenuItem>
                  )}
                </Select>
              </FormControl>
            </Box>
            
            <Box>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom color="secondary">
                    {isNewAgent ? 'Create New Agent' : 'Update Agent'}
                  </Typography>
                  
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <Box>
                      <TextField
                        fullWidth
                        label="Name"
                        name="name"
                        value={formData.name}
                        onChange={handleInputChange}
                        variant="outlined"
                        required
                      />
                    </Box>
                    
                    <Box>
                      <TextField
                        fullWidth
                        label="Description"
                        name="description"
                        value={formData.description}
                        onChange={handleInputChange}
                        variant="outlined"
                        multiline
                        rows={2}
                      />
                    </Box>
                    
                    <Box sx={{ width: { xs: '100%', sm: '50%' } }}>
                      <FormControl fullWidth variant="outlined" margin="normal">
                        <InputLabel id="model-select-label">Model *</InputLabel>
                        <Select
                          labelId="model-select-label"
                          id="model-select"
                          name="model"
                          value={formData.model}
                          onChange={handleModelChange}
                          label="Model *"
                          disabled={loadingModels}
                        >
                          {loadingModels ? (
                            <MenuItem disabled>
                              <CircularProgress size={20} sx={{ mr: 1 }} />
                              Loading models...
                            </MenuItem>
                          ) : modelDeployments.length > 0 ? (
                            modelDeployments.map(deployment => (
                              <MenuItem key={deployment.id} value={deployment.model}>
                                {deployment.name} ({deployment.model})
                              </MenuItem>
                            ))
                          ) : (
                            <MenuItem disabled>No models available</MenuItem>
                          )}
                        </Select>
                      </FormControl>
                    </Box>
                    
                    <Box>
                      <TextField
                        fullWidth
                        label="Instructions"
                        name="instructions"
                        value={formData.instructions}
                        onChange={handleInputChange}
                        variant="outlined"
                        multiline
                        rows={4}
                        required
                      />
                    </Box>
                    
                    <Box>
                      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                        <Button
                          variant="outlined"
                          onClick={resetForm}
                          disabled={loading}
                        >
                          Reset
                        </Button>
                        
                        <Button
                          variant="contained"
                          color="primary"
                          onClick={isNewAgent ? handleCreateAgent : handleUpdateAgent}
                          disabled={loading}
                          startIcon={loading ? <CircularProgress size={20} /> : (isNewAgent ? <AddIcon /> : <UpdateIcon />)}
                        >
                          {loading ? 'Processing...' : (isNewAgent ? 'Create Agent' : 'Update Agent')}
                        </Button>
                      </Box>
                    </Box>
                  </Box>
                </CardContent>
              </Card>
            </Box>
            
            <Box>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom color="secondary">
                    Agent Information
                  </Typography>
                  {formData.id ? (
                    <Box>
                      <Chip 
                        label={`ID: ${formData.id}`} 
                        color="default" 
                        variant="outlined" 
                        sx={{ mr: 1, mb: 1 }} 
                      />
                      <Chip 
                        label={`Name: ${formData.name}`} 
                        color="primary" 
                        variant="outlined" 
                        sx={{ mr: 1, mb: 1 }} 
                      />
                      <Chip 
                        label={`Model: ${formData.model}`} 
                        color="secondary" 
                        variant="outlined" 
                        sx={{ mr: 1, mb: 1 }} 
                      />
                    </Box>
                  ) : (
                    <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                      Select an agent or create a new one to see details
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Box>
            
            <Divider sx={{ my: 2 }} />

          </Box>
        </Paper>
        
        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={handleCloseSnackbar}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        >
          <Alert 
            onClose={handleCloseSnackbar} 
            severity={snackbar.severity} 
            sx={{ width: '100%' }}
          >
            {snackbar.message}
          </Alert>
        </Snackbar>
      </Container>
    </Box>
  );
};

export default AgentCreation;