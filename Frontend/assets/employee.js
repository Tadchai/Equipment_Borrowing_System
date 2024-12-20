const apiBaseUrl = 'http://localhost:5143/api/employee'; // เปลี่ยน URL ตาม backend ของคุณ

// Fetch all employees
async function fetchEmployees() {
    const response = await fetch(apiBaseUrl);
    const employees = await response.json();
    renderEmployees(employees);
}

// Add new employee
document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const name = document.getElementById('employeeName').value;

    const response = await fetch(apiBaseUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
    });

    if (response.ok) {
        alert('Employee added successfully');
        document.getElementById('createForm').reset();
        fetchEmployees();
    } else {
        alert('Error adding employee');
    }
});

// Search employees by name
document.getElementById('searchBtn').addEventListener('click', async () => {
    const name = document.getElementById('searchName').value;

    const response = await fetch(`${apiBaseUrl}/search/${name}`);
    const employees = await response.json();
    renderEmployees(employees);
});

// Render employee list
function renderEmployees(employees) {
    const list = document.getElementById('employeeList');
    list.innerHTML = '';
    employees.forEach((employee) => {
        const row = `
            <tr>
                <td>${employee.name}</td>
            </tr>
        `;
        list.insertAdjacentHTML('beforeend', row);
    });
}

fetchEmployees();
