import React from 'react';
import { Container, Typography } from '@mui/material';

const RagDemo: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Typography variant="h4" gutterBottom>
        RAG Demo
      </Typography>
      <Typography variant="body1">
        This is where the Retrieval Augmented Generation demo will be implemented.
      </Typography>
    </Container>
  );
};

export default RagDemo;