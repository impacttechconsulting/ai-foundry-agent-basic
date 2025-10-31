import React from 'react';
import {
  Toolbar,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  AutoFixHigh as AgentIcon,
  Logout as LogoutIcon,
  Search as SearchIcon,
  Description as DocumentIcon
} from '@mui/icons-material';

interface NavigationProps {
  currentPage: string;
  onNavigate: (page: string) => void;
  onLogout: () => void;
}

const Navigation: React.FC<NavigationProps> = ({ currentPage, onNavigate, onLogout }) => {
  return (
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
        {/* Main menu items */}
        <ListItem key="dashboard" disablePadding>
          <ListItemButton 
            onClick={() => onNavigate('dashboard')}
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
            onClick={() => onNavigate('chat')}
            selected={currentPage === 'chat'}
          >
            <ListItemIcon>
              <AgentIcon />
            </ListItemIcon>
            <ListItemText primary="Chat Agent" />
          </ListItemButton>
        </ListItem>
        
        {/* <ListItem key="agent-creation" disablePadding>
          <ListItemButton 
            onClick={() => onNavigate('agent-creation')}
            selected={currentPage === 'agent-creation'}
          >
            <ListItemIcon>
              <AgentIcon />
            </ListItemIcon>
            <ListItemText primary="Agent Creation" />
          </ListItemButton>
        </ListItem> */}
        
        <ListItem key="rag" disablePadding>
          <ListItemButton 
            onClick={() => onNavigate('rag')}
            selected={currentPage === 'rag'}
          >
            <ListItemIcon>
              <SearchIcon />
            </ListItemIcon>
            <ListItemText primary="RAG" />
          </ListItemButton>
        </ListItem>
        
        {/* <ListItem key="content" disablePadding>
          <ListItemButton 
            onClick={() => onNavigate('content')}
            selected={currentPage === 'content'}
          >
            <ListItemIcon>
              <DocumentIcon />
            </ListItemIcon>
            <ListItemText primary="Content Understanding" />
          </ListItemButton>
        </ListItem> */}
        
        {/* Divider and logout at the bottom */}
        <Divider sx={{ my: 2, mt: 'auto' }} />
        
        <ListItem key="logout" disablePadding>
          <ListItemButton onClick={onLogout}>
            <ListItemIcon>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText primary="Logout" />
          </ListItemButton>
        </ListItem>
      </List>
    </div>
  );
};

export default Navigation;