import React from 'react';
import { 
  Container, 
  Typography, 
  Paper, 
  Box, 
  Chip, 
  Card, 
  CardContent 
} from '@mui/material';
import { 
  Event as EventIcon, 
  LocationOn as LocationIcon, 
  CalendarToday as CalendarIcon 
} from '@mui/icons-material';

const Dashboard: React.FC = () => {
  return (
    <Box 
      sx={{ 
        minHeight: '100vh', 
        backgroundColor: '#f5f5f5',
        padding: 3
      }}
    >
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Paper 
          elevation={3} 
          sx={{ 
            p: 4, 
            backgroundColor: 'white',
            borderRadius: 4,
            boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
          }}
        >
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            <Typography variant="h3" component="h1" gutterBottom color="primary" sx={{ fontWeight: 'bold' }}>
              TechXConf 2025
            </Typography>
            
            <Typography variant="h5" component="h2" gutterBottom sx={{ color: '#1976d2', mb: 2 }}>
              Asia's Largest AI & Cloud Conference
            </Typography>
            
            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mb: 3, flexWrap: 'wrap' }}>
              <Chip 
                icon={<CalendarIcon />} 
                label="1 Day of Immersive Learning" 
                variant="outlined" 
                color="primary"
              />
              <Chip 
                icon={<EventIcon />} 
                label="100+ World-Class Experts" 
                variant="outlined" 
                color="secondary"
              />
              <Chip 
                icon={<LocationIcon />} 
                label="Global Tech Leaders" 
                variant="outlined" 
                color="primary"
              />
            </Box>
          </Box>
          
          <Typography variant="body1" paragraph align="center" sx={{ fontSize: '1.1rem', mb: 4, lineHeight: 1.7 }}>
            A technology event that brings together everyone in the dev/IT landscape, world-leading speakers, 
            industry leaders, large customers, partners, and thousands of delegates and offers an in-depth 
            technology learning and ideal professional networking environment for all attendees.
          </Typography>
          
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4, mt: 1 }}>
            <Box sx={{ flex: '1 1 45%', minWidth: 300 }} key="expect-card">
              <Card sx={{ height: '100%', borderRadius: 2, boxShadow: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="secondary" sx={{ fontWeight: 'bold' }}>
                    <EventIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                    What to Expect
                  </Typography>
                  <Typography variant="body2" sx={{ lineHeight: 1.7 }}>
                    Across the one day of immersive technology learning, attendees will get to learn from 
                    hundreds of educational sessions, dozens of real-life case studies, and panel discussions, 
                    delivered by 100+ world-class experts from across the world.
                  </Typography>
                </CardContent>
              </Card>
            </Box>
            <Box sx={{ flex: '1 1 45%', minWidth: 300 }} key="themes-card">
              <Card sx={{ height: '100%', borderRadius: 2, boxShadow: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="secondary" sx={{ fontWeight: 'bold' }}>
                    <LocationIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                    Key Themes
                  </Typography>
                  <Box component="ul" sx={{ pl: 2, mt: 1 }}>
                    <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                      <strong>App & Infra Modernization</strong>
                    </Typography>
                    <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                      <strong>Data & AI Innovation</strong>
                    </Typography>
                    <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                      <strong>Developer 2.0</strong> (Emerging Trends, Developer Productivity, Upskilling, Adopting Change)
                    </Typography>
                    <Typography component="li" variant="body2" sx={{ mb: 1 }}>
                      <strong>Hybrid Cloud</strong> (Edge to Cloud)
                    </Typography>
                    <Typography component="li" variant="body2">
                      <strong>Future is Now</strong> – Quantum, Blockchain, IoT, Meta, etc.
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Box>
          </Box>
          
          <Box sx={{ mt: 4, textAlign: 'center', p: 3, backgroundColor: 'rgba(25, 118, 210, 0.08)', borderRadius: 2 }}>
            <Typography variant="h6" color="primary" gutterBottom sx={{ fontWeight: 'bold' }}>
              Join thousands of technology professionals for cutting-edge content in both business and technology arenas.
            </Typography>
            <Typography variant="body1" sx={{ mt: 1 }}>
              TechXConf 2025 offers a perfect learning and professional opportunity for all attendees.
            </Typography>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default Dashboard;