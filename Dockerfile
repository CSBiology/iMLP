FROM mcr.microsoft.com/dotnet/sdk:5.0.202-focal-amd64

################## METADATA ######################
LABEL base_image="biocontainers:v1.2.0_cv1"
LABEL version="1.0"
LABEL software="iMLP"
LABEL software.version="0.1.0"
LABEL about.summary="iMLP is a LSTM for prediction of iMTS-L propensity profiles of proteins of interest"
LABEL about.home="https://github.com/CSBiology/iMLP"
LABEL about.documentation="https://github.com/CSBiology/iMLP"
LABEL about.license_file="https://github.com/CSBiology/iMLP/blob/main/LICENSE"
LABEL about.license="MIT"
LABEL about.tags="Sequence analysis"

################## MAINTAINERS ######################

LABEL author Kevin Schneider <schneike@bio.uni-kl.de>
LABEL author David Zimmer <davidzimmer91@gmail.com>
LABEL author Timo Mühlhaus <muehlhaus@bio.uni-kl.de>

#################### INSTALL ########################

RUN apt-get update -y \
    && apt-get install -y libnuma-dev \
    && apt-get install -y build-essential \
    && apt-get install -y g++

# Add openmpi libs
WORKDIR /usr/local

RUN wget https://www.open-mpi.org/software/ompi/v1.10/downloads/openmpi-1.10.3.tar.gz \
    && tar -xzvf openmpi-1.10.3.tar.gz \
    && rm -f openmpi-1.10.3.tar.gz

WORKDIR /usr/local/openmpi-1.10.3

RUN ./configure --prefix=/usr/local/mpi 
RUN make -j all
RUN make install

ENV PATH=/usr/local/mpi/bin:$PATH
ENV LD_LIBRARY_PATH=/usr/local/mpi/lib:$LD_LIBRARY_PATH

# Add cntk libs
WORKDIR /usr/local

RUN wget https://cntk.azurewebsites.net/BinaryDrop/CNTK-2-7-Linux-64bit-CPU-Only.tar.gz && \
    tar -xzf CNTK-2-7-Linux-64bit-CPU-Only.tar.gz && \
    rm -f CNTK-2-7-Linux-64bit-CPU-Only.tar.gz

RUN cp /usr/local/cntk/cntk/lib/Cntk.Core.CSBinding-2.7.so /usr/local/cntk/cntk/lib/libCntk.Core.CSBinding-2.7.dll

ENV PATH="/usr/local/cntk/cntk/lib:${PATH}"
ENV PATH="/usr/local/cntk/dependencies/lib:${PATH}"
ENV LD_LIBRARY_PATH="/usr/local/cntk/cntk/dependencies/lib:${LD_LIBRARY_PATH}"
ENV LD_LIBRARY_PATH="/usr/local/cntk/cntk/dependencies/lib:${LD_LIBRARY_PATH}"

## Clone imlp repo, replace with binary release later eventually

Add ./cli-tool /usr/local/imlp

WORKDIR /usr/local/imlp
RUN dotnet tool restore
RUN dotnet fake build
Run ls
ENV PATH="/usr/local/imlp/bin/iMLP/net5.0:${PATH}"
ENV LD_LIBRARY_PATH="/usr/local/imlp/bin/iMLP/net5.0:${PATH}:${LD_LIBRARY_PATH}"

WORKDIR /usr/local/imlp/bin/iMLP/net5.0
