const urlParams = new URLSearchParams(window.location.search);
const employeeId = urlParams.get('id');

async function fetchEmployeeData() {
  try {
    const response = await fetch(`http://localhost:5143/Employee/GetById?id=${employeeId}`); 
    if (response.ok) {
      const employee = await response.json();
      document.getElementById('fullName').value = employee.fullName;
    } else {
      alert('Failed to fetch employee data.');
    }
  } catch (error) {
    console.error('Error fetching employee data:', error);
    alert('An error occurred while fetching employee data.');
  }
}

fetchEmployeeData();

document.getElementById('editEmployeeForm').addEventListener('submit', async (e) => {
  e.preventDefault();

  const fullName = document.getElementById('fullName').value;

  try {
    const response = await fetch('http://localhost:5143/Employee/Update', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        Id: employeeId,
        FullName: fullName,
      }),
    });

    const result = await response.json();

    if (response.ok) {
      alert(result.Message || 'Employee updated successfully.');
      window.location.href = `employeeDetail.html?id=${employeeId}`; 
    } else {
      alert(result.Message || 'Failed to update employee.');
    }
  } catch (error) {
    console.error('Error updating employee:', error);
    alert('An error occurred while updating the employee.');
  }
});