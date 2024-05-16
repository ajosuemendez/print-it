import http from 'k6/http';
import { sleep, check } from 'k6';

// Read the file data in the global scope
const fileData = open('test.pdf', 'b');

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '10s', target: 5},
        { duration: '5m', target: 5},
        { duration: '1m', target: 0},
    ],
};

const params = { timeout: 10000 };

const API_BASE_URL = 'http://localhost:7000';

export default () => {
    const formData = {
        PrinterPath: 'PDFCreator',
        PdfFile: http.file(fileData, 'test.pdf'),
        DocumentName: "test.pdf",
        PrintToFile: false
    };

    const response = http.post(`${API_BASE_URL}/print/from-pdf`, formData, params);

    // Check if the request was successful
    check(response, {
        'Status is 200': (r) => r.status === 200,
    });

    sleep(1);
};
