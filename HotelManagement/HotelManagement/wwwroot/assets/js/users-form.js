(function () {
    var roleSelect = document.getElementById('roleSelect');
    var staffGroup = document.getElementById('staffGroup');
    if (!roleSelect || !staffGroup) return;

    function toggleStaffGroup() {
        var showStaff = roleSelect.value === 'Admin' || roleSelect.value === 'Staff';
        staffGroup.style.display = showStaff ? '' : 'none';
    }

    roleSelect.addEventListener('change', toggleStaffGroup);
    toggleStaffGroup();
})();
