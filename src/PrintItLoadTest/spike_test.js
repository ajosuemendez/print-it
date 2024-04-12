import http from 'k6/http';
import { sleep, check } from 'k6';

const fileData = open('test.pdf', 'b');

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '10s', target: 1},
        { duration: '1m', target: 1},
        { duration: '10s', target: 25},
        { duration: '2m', target: 25},
        { duration: '10s', target: 1},
        { duration: '1m', target: 1},
        { duration: '10s', target: 0},
    ]
};

const API_BASE_URL = 'http://localhost:7000';

export default () => {
    const formData = {
        PrinterPath: 'PDFCreator',
        PdfFile: http.file(fileData, 'test.pdf')
    };

    const response = http.post(`${API_BASE_URL}/print/from-pdf`, formData);

    // Check if the request was successful
    check(response, {
        'Status is 200': (r) => r.status === 200,
    });

    sleep(1);
};