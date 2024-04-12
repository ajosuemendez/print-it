import http from 'k6/http';
import { sleep, check } from 'k6';

// Read the file data in the global scope
const fileData = open('test.pdf', 'b');

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '1m', target: 20},
        { duration: '5m', target: 20},
        { duration: '1m', target: 0},
    ],
    // thresholds: {
    //     http_req_duration: ['p(99)<100'] // 99% must be below 100ms
    // }
};

const API_BASE_URL = 'http://localhost:7000';

export default () => {
    const formData = {
        PrinterPath: 'Brother Color Leg Type1 Class Driver',
        PdfFile: http.file(fileData, 'test.pdf'),
        DocumentName: "test.pdf",
        PrintToFile: true
    };

    const response = http.post(`${API_BASE_URL}/print/from-pdf`, formData);

    // Check if the request was successful
    check(response, {
        'Status is 200': (r) => r.status === 200,
    });

    sleep(1);
};
