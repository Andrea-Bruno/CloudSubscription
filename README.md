# CloudSubscription Configuration Guide

## Overview:
CloudSubscription is a White Label project designed to serve as a foundation for subscription-based services. 
The concept is to allow businesses to add their own branding, advertising, and customizations, enabling them 
to sell services under their own commercial brand. This project provides the core functionality, while you 
can focus on tailoring the user experience to match your business identity.

---

## SSL/TLS Certificate Configuration for HTTPS

To ensure secure communication, you need to configure an SSL/TLS certificate for HTTPS.

### Step 1: Obtain a Certificate
- For production environments, use a valid SSL/TLS certificate from a trusted Certificate Authority (e.g., Let's Encrypt).
- For development environments, generate a self-signed certificate using the following command:
  dotnet dev-certs https --trust

### Step 2: Place the Certificate
- On Linux systems, place the `.pfx` certificate file in the standard directory:
  /etc/ssl/private/
  Example: /etc/ssl/private/mycertificate.pfx

### Step 3: Update appsettings.json
Edit the `appsettings.json` file to include the certificate path and password. Example:
  {
    "Kestrel": {
      "Endpoints": {
        "Https": {
          "Url": "https://*:443",
          "Certificate": {
            "Path": "/etc/ssl/private/mycertificate.pfx",
            "Password": "your-certificate-password"
          }
        }
      }
    }
  }

### Step 4: Verify the Configuration
- Ensure that the firewall allows traffic on port 443.
- Start the application and verify that it is accessible via HTTPS.

---

## Configure PayPal Business Email

To enable PayPal payments, you must configure your PayPal Business email address.

### Step 1: Update appsettings.json
Open the `appsettings.json` file and replace the placeholder email with your PayPal Business email address. Example:
  {
    "PayPalBusinessEmail": "your-business-email@example.com"
  }

### Step 2: Verify
- Ensure the email address is correct and corresponds to your PayPal Business account.
- Save the file and restart the application.

---

## API Endpoint Configuration

The application communicates with a cloud server via an API. Ensure the API endpoint is correctly configured in the `appsettings.json` file.

### Step 1: Update appsettings.json
  {
    "ApiEndpoint": "https://yourserver.com/api"
  }

### Step 2: Use HTTPS
- If the server is outside your intranet, it is recommended to use HTTPS for secure communication.

---

## White Label Customization:
This project is designed as a White Label solution, allowing you to:
- Add your branding (e.g., logos, colors, and themes).
- Integrate advertising and promotional content.
- Customize the user interface and functionality to align with your business needs.

By leveraging this project, you can quickly launch a subscription-based service under your own brand, saving time and development costs.

---

Important Notes:
- Certificates: Always use valid SSL/TLS certificates in production to ensure secure communication.
- PayPal Email: The configured email must be a valid PayPal Business account.
- Security: Protect sensitive information, such as the certificate password, and ensure proper access controls.

---

## Installation on Linux

To install the application on a Linux system, follow these steps:

   **Set Execution Permissions**:
   Run the following command to give execution permissions to the `install.sh` script:
    
   chmod +x install.sh
   
   **Run the Installation Script**:
   Execute the script with superuser privileges:

   sudo ./install.sh

---

## License:
This project is licensed under the terms specified in the `LICENSE.txt` file. Ensure compliance with the license when using or distributing this project.

---

For further assistance or inquiries, feel free to contact the support team or refer to the official documentation.
