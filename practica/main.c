#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>

#define BUFFER_SIZE 4096

int main(int argc, char *argv[]) {
    if (argc != 3) {
        fprintf(stderr, "Использование: %s <источник> <назначение>\n", argv[0]);
        exit(EXIT_FAILURE);
    }

    int src_fd = open(argv[1], O_RDONLY);
    if (src_fd == -1) {
        perror("Ошибка открытия исходного файла");
        exit(EXIT_FAILURE);
    }

    // O_WRONLY - только запись, O_CREAT - создать если нет, O_TRUNC - перезаписать
    int dest_fd = open(argv[2], O_WRONLY | O_CREAT | O_TRUNC, 0644);
    if (dest_fd == -1) {
        perror("Ошибка открытия/создания целевого файла");
        close(src_fd);
        exit(EXIT_FAILURE);
    }

    char buffer[BUFFER_SIZE];
    ssize_t bytes_read, bytes_written;

    while ((bytes_read = read(src_fd, buffer, BUFFER_SIZE)) > 0) {
        bytes_written = write(dest_fd, buffer, bytes_read);
        if (bytes_written != bytes_read) {
            perror("Ошибка записи");
            break;
        }
    }

    if (bytes_read == -1) perror("Ошибка чтения");

    close(src_fd);
    close(dest_fd);

    return 0;
}