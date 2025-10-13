import React from 'react';
import { Container, Typography } from '@mui/material';

const Dashboard: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Typography variant="body1">
        Welcome to the AI Foundry Dashboard. This is where you can manage your AI services.
      </Typography>
    </Container>
  );
};

export default Dashboard;