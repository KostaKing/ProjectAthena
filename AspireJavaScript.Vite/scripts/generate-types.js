#!/usr/bin/env node

import { execSync } from 'child_process';
import { existsSync, mkdirSync } from 'fs';
import { dirname } from 'path';

// Ensure types directory exists
const typesDir = 'src/types';
if (!existsSync(typesDir)) {
    mkdirSync(typesDir, { recursive: true });
}

// Get the API URL from environment variables set by Aspire
const apiUrl = process.env.services__weatherapi__https__0 || 
              process.env.services__weatherapi__http__0;

if (!apiUrl) {
    console.error('❌ API URL not found in environment variables.');
    console.error('Make sure the Aspire app is running and the weatherapi service is available.');
    process.exit(1);
}

const swaggerUrl = `${apiUrl}/swagger/v1/swagger.json`;
console.log(`🔄 Generating types from: ${swaggerUrl}`);

try {
    execSync(`npx openapi-typescript "${swaggerUrl}" -o src/types/api.ts`, {
        stdio: 'inherit',
        env: {
            ...process.env,
            NODE_TLS_REJECT_UNAUTHORIZED: '0', // ✅ <- bypass self-signed cert rejection
        },
    });
    console.log('✅ TypeScript types generated successfully!');
} catch (error) {
    console.error('❌ Failed to generate types:', error.message);
    process.exit(1);
}
