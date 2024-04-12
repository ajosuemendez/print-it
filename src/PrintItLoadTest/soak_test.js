import http from 'k6/http';
import { sleep, check } from 'k6';

const fileData = open('test.pdf', 'b');

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '1m', target: 14},
        { duration: '1h', target: 14},
        { duration: '1m', target: 0},
    ]
};

const API_BASE_URL = 'http://localhost:7000';

export default () => {
    const formData = {
        PrinterPath: 'Brother Color Leg Type1 Class Driver',
        PdfFile: http.file(fileData, 'test.pdf')
    };

    const response = http.post(`${API_BASE_URL}/print/from-pdf`, formData);

    // Check if the request was successful
    check(response, {
        'Status is 200': (r) => r.status === 200,
    });

    sleep(1);
};