const apiBaseUrl = "http://localhost:5143/Employee";
const employeeTableBody = document.getElementById("employeeTableBody");
const searchText = document.getElementById("searchText");
const searchButton = document.getElementById("searchButton");
const pageNumber = document.getElementById("pageNumber");
const pageSize = document.getElementById("pageSize");
const totalRows = document.getElementById("totalRows");
document.getElementById("addEmployee").addEventListener("click", function () {
    window.location.href = "/Frontend/addemployee.html"; 
});

let currentPage = 1;

async function fetchEmployees()
{
    const input = {
        Name: searchText.value || null,
        Page: currentPage - 1,
        PageSize: parseInt(pageSize.value),
    };

    try
    {
        const response = await fetch(`${apiBaseUrl}/Search`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(input),
        });

        if (!response.ok) throw new Error("Error fetching employees");

        const data = await response.json();
        renderEmployees(data);
    } catch (error)
    {
        console.error(error);
        alert("Failed to fetch employees");
    }
}

function renderEmployees(response)
{
    employeeTableBody.innerHTML = "";

    response.data.forEach((employee) =>
    {
        const row = document.createElement("tr");
        row.innerHTML = `
                    <td><a href="/Frontend/employeedetail.html?id=${employee.employeeId}">${employee.name}</a></td>
                `;
        employeeTableBody.appendChild(row);
    });

    updatePagination(response);
}

function updatePagination(response)
{
    totalRows.textContent = `Total Rows: ${response.rowCount}`;
    const totalPages = Math.ceil(response.rowCount / parseInt(pageSize.value));

    pageNumber.innerHTML = "";
    for (let i = 1; i <= totalPages; i++)
    {
        const option = document.createElement("option");
        option.value = i;
        option.textContent = i;
        if (i === currentPage) option.selected = true;
        pageNumber.appendChild(option);
    }
}

searchButton.addEventListener("click", fetchEmployees);

pageSize.addEventListener("change", () =>
{
    currentPage = 1;
    fetchEmployees();
});

pageNumber.addEventListener("change", () =>
{
    currentPage = parseInt(pageNumber.value);
    fetchEmployees();
});

window.onload = fetchEmployees;
