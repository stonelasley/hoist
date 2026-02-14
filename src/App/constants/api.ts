import { Platform } from 'react-native';

const DEV_API_URL = Platform.select({
  android: 'https://10.0.2.2:5001',
  default: 'https://localhost:5001',
});

const PROD_API_URL = 'https://api.hoist.app';

export const API_BASE_URL = __DEV__ ? DEV_API_URL : PROD_API_URL;

export const GOOGLE_CLIENT_ID = '<your-google-client-id>';
