FROM python:3.10-slim
RUN apt update && apt install -y sysbench git curl procps \
    && pip install --upgrade pip \
    && pip install jupyter matplotlib psutil
WORKDIR /app
COPY . .
EXPOSE 8888
CMD ["jupyter", "notebook", "--ip=0.0.0.0", "--allow-root", "--NotebookApp.token=''"]
