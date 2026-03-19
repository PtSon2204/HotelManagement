$('#deleteModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var name = button.data('name');

    $('#deleteStaffName').text(name);
    $('#deleteForm').attr('action', '/Admin/Staffs/Delete/' + id);
});
